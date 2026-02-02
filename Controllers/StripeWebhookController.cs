using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Services;
using Stripe;
using System.Text;

namespace MyPhotoBiz.Controllers
{
    [ApiController]
    [Route("webhooks/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IInvoiceService _invoiceService;
        private readonly IConfiguration _configuration;

        public StripeWebhookController(
            ILogger<StripeWebhookController> logger,
            IInvoiceService invoiceService,
            IConfiguration configuration)
        {
            _logger = logger;
            _invoiceService = invoiceService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            Event stripeEvent;

            try
            {
                if (string.IsNullOrWhiteSpace(webhookSecret))
                {
                    _logger.LogWarning("Stripe webhook received but secret is not configured.");
                    return StatusCode(501);
                }

                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
                return BadRequest();
            }

            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    await HandlePaymentIntentSucceeded(stripeEvent);
                    break;
                case Events.PaymentIntentPaymentFailed:
                    await HandlePaymentFailed(stripeEvent);
                    break;
                case Events.ChargeRefunded:
                    await HandleRefunded(stripeEvent);
                    break;
                default:
                    // Ignore other events
                    break;
            }

            return Ok();
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent) return;
            var invoiceIdString = intent.Metadata.GetValueOrDefault("invoiceId");
            if (!int.TryParse(invoiceIdString, out var invoiceId)) return;

            try
            {
                var amount = intent.AmountReceived / 100m;
                var paidDate = DateTime.UtcNow;
                await _invoiceService.ApplyPaymentAsync(
                    invoiceId,
                    amount,
                    paidDate,
                    MyPhotoBiz.Enums.PaymentMethod.CreditCard,
                    intent.Id,
                    intent.LatestChargeId,
                    intent.Description,
                    processedByUserId: null);

                await _invoiceService.UpdateInvoiceStatusAsync(invoiceId, InvoiceStatus.Paid, paidDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark invoice {InvoiceId} paid from webhook", invoiceId);
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent) return;
            var invoiceIdString = intent.Metadata.GetValueOrDefault("invoiceId");
            if (!int.TryParse(invoiceIdString, out var invoiceId)) return;

            try
            {
                await _invoiceService.UpdateInvoiceStatusAsync(invoiceId, InvoiceStatus.Pending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record failed payment for invoice {InvoiceId}", invoiceId);
            }
        }

        private async Task HandleRefunded(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Charge charge) return;
            var invoiceIdString = charge.Metadata.GetValueOrDefault("invoiceId");
            if (!int.TryParse(invoiceIdString, out var invoiceId)) return;

            try
            {
                var refund = charge.Refunds?.FirstOrDefault();
                var refundAmount = (refund?.Amount ?? charge.AmountRefunded) / 100m;
                var reason = refund?.Reason ?? "Refunded via Stripe";

                await _invoiceService.IssueRefundAsync(
                    invoiceId,
                    refundAmount,
                    reason,
                    MyPhotoBiz.Enums.PaymentMethod.CreditCard,
                    transactionId: refund?.Id ?? charge.Id,
                    notes: reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record refund for invoice {InvoiceId}", invoiceId);
            }
        }
    }
}
