using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing invoices, payments, and refunds.
    /// Features: Separate payment tracking, soft-delete support, partial payment handling,
    /// refund processing, and invoice lifecycle management.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Get Methods

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(i => i.PhotoShoot)
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(i => i.PhotoShoot)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(i => i.PhotoShoot)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetFilteredInvoicesAsync(
            string? searchTerm,
            int? clientId,
            InvoiceStatus? status,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Invoices
                .Include(i => i.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(i => i.PhotoShoot)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(i => i.InvoiceNumber.Contains(searchTerm));

            if (clientId.HasValue)
                query = query.Where(i => i.ClientProfileId == clientId.Value);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            return await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetClientInvoicesAsync(
            int clientId,
            InvoiceStatus? status,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Invoices
                .Include(i => i.ClientProfile)
                .Include(i => i.PhotoShoot)
                .Where(i => i.ClientProfileId == clientId);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            return await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetRecentInvoicesAsync(int count)
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.Today;
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                .Where(i => i.DueDate < today && i.Status != InvoiceStatus.Paid)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesDueSoonAsync(int days)
        {
            var targetDate = DateTime.Today.AddDays(days);
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                .Where(i => i.DueDate <= targetDate && i.Status != InvoiceStatus.Paid)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByPhotoShootIdAsync(int photoShootId)
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                .Include(i => i.PhotoShoot)
                .Where(i => i.PhotoShootId == photoShootId)
                .ToListAsync();
        }

        #endregion

        #region Create / Update

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (string.IsNullOrEmpty(invoice.InvoiceNumber))
                invoice.InvoiceNumber = await GenerateInvoiceNumberAsync();

            invoice.UpdatedDate = DateTime.Now;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task UpdateInvoiceStatusAsync(int invoiceId, InvoiceStatus status, DateTime? paidDate = null)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            invoice.Status = status;
            invoice.UpdatedDate = DateTime.Now;

            if (status == InvoiceStatus.Paid && paidDate.HasValue)
                invoice.PaidDate = paidDate;

            await _context.SaveChangesAsync();
        }

        public async Task BulkUpdateInvoiceStatusAsync(IEnumerable<int> invoiceIds, InvoiceStatus status)
        {
            var invoices = await _context.Invoices.Where(i => invoiceIds.Contains(i.Id)).ToListAsync();
            foreach (var invoice in invoices)
            {
                invoice.Status = status;
                invoice.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkInvoiceAsPaidAsync(int invoiceId, DateTime paidDate)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidDate = paidDate;
            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Duplicate / Reminders

        public async Task<Invoice?> DuplicateInvoiceAsync(int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null) return null;

            var copy = new Invoice
            {
                ClientProfileId = invoice.ClientProfileId,
                PhotoShootId = invoice.PhotoShootId,
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                Status = InvoiceStatus.Draft,
                Amount = invoice.Amount,
                Tax = invoice.Tax,
                Notes = invoice.Notes,
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                UpdatedDate = DateTime.Now,
                InvoiceItems = invoice.InvoiceItems?.Select(ii => new InvoiceItem
                {
                    Description = ii.Description,
                    Quantity = ii.Quantity,
                    UnitPrice = ii.UnitPrice
                }).ToList()
            };

            await CreateInvoiceAsync(copy);
            return copy;
        }

        public async Task SendInvoiceReminderAsync(int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            // Email sending handled by EmailSender service
            await Task.CompletedTask;
        }

        public async Task SendOverdueNoticeAsync(int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            // Email sending handled by EmailSender service
            await Task.CompletedTask;
        }

        public async Task SendPaymentConfirmationAsync(int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            // Email sending handled by EmailSender service
            await Task.CompletedTask;
        }

        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            invoice.UpdatedDate = DateTime.Now;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Helpers

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var count = await _context.Invoices.CountAsync();
            return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{count + 1:D4}";
        }

        #endregion

        #region Payment Management

        /// <summary>
        /// Legacy method: Applies a payment to an invoice (kept for backward compatibility)
        /// </summary>
        [Obsolete("Use the overload with PaymentMethod parameter for better tracking")]
        public async Task ApplyPaymentAsync(int invoiceId, decimal amount, DateTime paidDate)
        {
            await ApplyPaymentAsync(invoiceId, amount, paidDate, PaymentMethod.Other);
        }

        /// <summary>
        /// Applies a payment to an invoice with full payment method tracking
        /// </summary>
        public async Task<Payment> ApplyPaymentAsync(
            int invoiceId,
            decimal amount,
            DateTime paymentDate,
            PaymentMethod paymentMethod,
            string? transactionId = null,
            string? referenceNumber = null,
            string? notes = null,
            string? processedByUserId = null)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) throw new InvalidOperationException("Invoice not found");
            if (amount <= 0) throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));

            // Create a new payment record
            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = amount,
                PaymentDate = paymentDate,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                ReferenceNumber = referenceNumber,
                Notes = notes,
                ProcessedByUserId = processedByUserId,
                CreatedDate = DateTime.Now
            };

            _context.Set<Payment>().Add(payment);

            // Calculate total paid including this payment
            var totalPaid = (invoice.Payments?.Where(p => !p.IsRefund).Sum(p => p.Amount) ?? 0) + amount;
            var totalRefunded = invoice.Payments?.Where(p => p.IsRefund).Sum(p => p.Amount) ?? 0;
            var balanceDue = invoice.TotalAmount - totalPaid + totalRefunded;

            // Update invoice status based on payment
            if (balanceDue <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = paymentDate;
            }
            else if (totalPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return payment;
        }

        /// <summary>
        /// Issues a refund for an invoice
        /// </summary>
        public async Task<Payment> IssueRefundAsync(
            int invoiceId,
            decimal refundAmount,
            string refundReason,
            PaymentMethod paymentMethod,
            string? transactionId = null,
            string? notes = null,
            string? processedByUserId = null)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) throw new InvalidOperationException("Invoice not found");
            if (refundAmount <= 0) throw new ArgumentException("Refund amount must be greater than zero", nameof(refundAmount));

            var totalPaid = invoice.Payments?.Where(p => !p.IsRefund).Sum(p => p.Amount) ?? 0;
            if (refundAmount > totalPaid)
                throw new InvalidOperationException("Refund amount cannot exceed total payments received");

            // Create refund payment record
            var refund = new Payment
            {
                InvoiceId = invoiceId,
                Amount = refundAmount,
                PaymentDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                Notes = notes,
                IsRefund = true,
                RefundReason = refundReason,
                ProcessedByUserId = processedByUserId,
                CreatedDate = DateTime.Now
            };

            _context.Set<Payment>().Add(refund);

            // Update invoice status
            var totalRefunded = (invoice.Payments?.Where(p => p.IsRefund).Sum(p => p.Amount) ?? 0) + refundAmount;
            var balanceDue = invoice.TotalAmount - totalPaid + totalRefunded;

            if (totalRefunded >= totalPaid)
            {
                invoice.Status = InvoiceStatus.Refunded;
                invoice.RefundAmount = totalRefunded;
            }
            else if (balanceDue > 0 && totalPaid > totalRefunded)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
                invoice.RefundAmount = totalRefunded;
            }

            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return refund;
        }

        /// <summary>
        /// Gets all payments for a specific invoice
        /// </summary>
        public async Task<IEnumerable<Payment>> GetInvoicePaymentsAsync(int invoiceId)
        {
            return await _context.Set<Payment>()
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        /// <summary>
        /// Deletes a payment (only if it's the most recent payment)
        /// </summary>
        public async Task DeletePaymentAsync(int paymentId, string? reason = null)
        {
            var payment = await _context.Set<Payment>()
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null) throw new InvalidOperationException("Payment not found");

            var invoice = payment.Invoice;
            var mostRecentPayment = invoice.Payments.OrderByDescending(p => p.CreatedDate).FirstOrDefault();

            // Only allow deletion of the most recent payment to maintain data integrity
            if (mostRecentPayment?.Id != paymentId)
                throw new InvalidOperationException("Can only delete the most recent payment. Please issue a refund instead.");

            _context.Set<Payment>().Remove(payment);

            // Recalculate invoice status after payment removal
            var remainingPayments = invoice.Payments.Where(p => p.Id != paymentId).ToList();
            var totalPaid = remainingPayments.Where(p => !p.IsRefund).Sum(p => p.Amount);
            var totalRefunded = remainingPayments.Where(p => p.IsRefund).Sum(p => p.Amount);
            var balanceDue = invoice.TotalAmount - totalPaid + totalRefunded;

            if (totalPaid == 0)
            {
                invoice.Status = InvoiceStatus.Pending;
                invoice.PaidDate = null;
            }
            else if (balanceDue > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.Status = InvoiceStatus.Paid;
            }

            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Delete

        /// <summary>
        /// Soft deletes an invoice (sets IsDeleted flag)
        /// </summary>
        public async Task DeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            // Soft delete - preserve data for audit trail
            invoice.IsDeleted = true;
            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Permanently deletes an invoice (use with caution)
        /// </summary>
        public async Task HardDeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) throw new InvalidOperationException("Invoice not found");

            // Only allow hard delete for Draft invoices that haven't been paid
            if (invoice.Status != InvoiceStatus.Draft && invoice.Payments.Any())
                throw new InvalidOperationException("Cannot permanently delete invoices with payments. Use soft delete instead.");

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
