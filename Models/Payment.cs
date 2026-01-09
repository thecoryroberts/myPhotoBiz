using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents an individual payment made towards an invoice.
    /// Supports partial payments, multiple payment methods, and refunds.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Transaction ID from payment gateway (Stripe, PayPal, etc.)
        /// </summary>
        [StringLength(255)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Reference number for manual payments (check number, bank reference, etc.)
        /// </summary>
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Indicates if this is a refund (negative amount applied to invoice)
        /// </summary>
        public bool IsRefund { get; set; } = false;

        /// <summary>
        /// Reason for refund if IsRefund is true
        /// </summary>
        [StringLength(500)]
        public string? RefundReason { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign key - Invoice
        [Required]
        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;

        // Optional: Who processed this payment
        public string? ProcessedByUserId { get; set; }
        public virtual ApplicationUser? ProcessedByUser { get; set; }
    }
}
