// Controllers/PrintOrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    public class PrintOrderController : Controller
    {
        private readonly IPrintOrderService _printOrderService;
        private readonly ILogger<PrintOrderController> _logger;

        public PrintOrderController(
            IPrintOrderService printOrderService,
            ILogger<PrintOrderController> logger)
        {
            _printOrderService = printOrderService;
            _logger = logger;
        }

        #region Admin Actions

        /// <summary>
        /// Admin view to list and manage all print orders
        /// </summary>
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Index(OrderStatus? status = null, string? search = null)
        {
            var orders = await _printOrderService.GetOrdersAsync(status, searchTerm: search);
            var stats = await _printOrderService.GetOrderStatsAsync();

            ViewBag.Stats = stats;
            ViewBag.CurrentStatus = status;
            ViewBag.SearchTerm = search;

            return View(orders);
        }

        /// <summary>
        /// Admin view for order details
        /// </summary>
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _printOrderService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            ViewBag.ShippingCost = _printOrderService.GetShippingCost();
            return View(order);
        }

        /// <summary>
        /// Process an order (move to Processing status)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Process(int id, string? printLabOrderId = null)
        {
            var result = await _printOrderService.ProcessOrderAsync(id, printLabOrderId);

            if (result.Success)
                TempData["Success"] = $"Order {result.Data?.OrderNumber} is now being processed.";
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Ship an order (move to Shipped status)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Ship(int id, string? trackingNumber = null)
        {
            var result = await _printOrderService.ShipOrderAsync(id, trackingNumber);

            if (result.Success)
                TempData["Success"] = $"Order {result.Data?.OrderNumber} has been shipped.";
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Mark order as delivered
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Deliver(int id)
        {
            var result = await _printOrderService.DeliverOrderAsync(id);

            if (result.Success)
                TempData["Success"] = $"Order {result.Data?.OrderNumber} has been delivered.";
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A cancellation reason is required.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _printOrderService.CancelOrderAsync(id, reason);

            if (result.Success)
                TempData["Success"] = $"Order {result.Data?.OrderNumber} has been cancelled.";
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Mark order as paid
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var result = await _printOrderService.MarkOrderPaidAsync(id);

            if (result.Success)
                TempData["Success"] = $"Order {result.Data?.OrderNumber} has been marked as paid.";
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Refund an order
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(int id)
        {
            var result = await _printOrderService.RefundOrderAsync(id);

            if (result.Success)
                TempData["Success"] = $"Order {result.Data?.OrderNumber} has been refunded.";
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Client Actions (Public)

        /// <summary>
        /// Display cart preview with favorite photos for ordering
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CartPreview(string sessionToken)
        {
            try
            {
                var (isValid, errorMessage, session) = await _printOrderService.ValidateSessionAsync(sessionToken);

                if (!isValid || session == null)
                {
                    TempData["Error"] = errorMessage ?? "Invalid session.";
                    return RedirectToAction("Index", "Gallery");
                }

                var favorites = await _printOrderService.GetCartPhotosAsync(session.Id);
                var printPrices = await _printOrderService.GetPricingAsync();

                ViewBag.SessionToken = sessionToken;
                ViewBag.GalleryName = session.Gallery.Name;
                ViewBag.BrandColor = session.Gallery.BrandColor ?? "#2c3e50";
                ViewBag.PrintPrices = printPrices;
                ViewBag.ShippingCost = _printOrderService.GetShippingCost();

                return View(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart preview");
                TempData["Error"] = "An error occurred while loading your cart. Please try again.";
                return RedirectToAction("Index", "Gallery");
            }
        }

        /// <summary>
        /// Place a print order
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PrintOrderViewModel model, string sessionToken)
        {
            try
            {
                var (isValid, errorMessage, session) = await _printOrderService.ValidateSessionAsync(sessionToken);

                if (!isValid || session == null)
                    return Unauthorized();

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill in all required fields correctly.";
                    return RedirectToAction("CartPreview", new { sessionToken });
                }

                var result = await _printOrderService.CreateOrderAsync(session.Id, model);

                if (result.Success && result.Data != null)
                {
                    _logger.LogInformation("Order {OrderNumber} created for {ClientEmail}",
                        result.Data.OrderNumber, result.Data.ClientEmail);
                    return RedirectToAction("OrderConfirmation", new { orderId = result.Data.Id });
                }

                TempData["Error"] = result.ErrorMessage ?? "An error occurred while placing your order.";
                return RedirectToAction("CartPreview", new { sessionToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("CartPreview", new { sessionToken });
            }
        }

        /// <summary>
        /// Display order confirmation (public for clients)
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            try
            {
                var order = await _printOrderService.GetOrderAsync(orderId);

                if (order == null)
                    return NotFound();

                ViewBag.BrandColor = order.Session?.Gallery?.BrandColor ?? "#2c3e50";
                ViewBag.GalleryName = order.Session?.Gallery?.Name ?? "Photo Gallery";
                ViewBag.ShippingCost = _printOrderService.GetShippingCost();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order confirmation");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get order details via API for AJAX (Admin only)
        /// </summary>
        [HttpGet]
        [Route("api/printorder/{orderId}")]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _printOrderService.GetOrderAsync(orderId);

                if (order == null)
                    return NotFound();

                return Ok(new
                {
                    order.Id,
                    order.OrderNumber,
                    order.ClientName,
                    order.ClientEmail,
                    order.ClientPhone,
                    order.TotalPrice,
                    Status = order.Status.ToString(),
                    order.CreatedDate,
                    ShippingCost = _printOrderService.GetShippingCost(),
                    Items = order.Items.Select(i => new
                    {
                        i.Id,
                        PhotoTitle = i.Photo?.Title,
                        PhotoUrl = i.Photo?.FilePath,
                        i.Size,
                        i.FinishType,
                        i.Quantity,
                        i.UnitPrice,
                        Total = i.UnitPrice * i.Quantity
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details");
                return StatusCode(500, new { message = "An error occurred while retrieving order details." });
            }
        }

        #endregion
    }
}
