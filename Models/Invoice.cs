using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Tax cannot be negative")]
        public decimal Tax { get; set; }

        public string? Notes { get; set; }

        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Amount refunded to client
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal RefundAmount { get; set; } = 0;

        /// <summary>
        /// Soft delete flag to preserve history
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Currency code (ISO 4217) for international clients
        /// </summary>
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Last date a payment reminder was sent
        /// </summary>
        public DateTime? ReminderSentDate { get; set; }

        /// <summary>
        /// Indicates if this is a recurring invoice
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Recurrence pattern (Monthly, Weekly, etc.) - stored as string for flexibility
        /// </summary>
        [StringLength(50)]
        public string? RecurrencePattern { get; set; }

        /// <summary>
        /// Next date for recurring invoice generation
        /// </summary>
        public DateTime? NextRecurrenceDate { get; set; }

        /// <summary>
        /// Required deposit amount before shoot
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal DepositAmount { get; set; } = 0;

        /// <summary>
        /// Whether the deposit has been paid
        /// </summary>
        public bool DepositPaid { get; set; } = false;

        /// <summary>
        /// Date the deposit was paid
        /// </summary>
        public DateTime? DepositPaidDate { get; set; }

        // Client Navigation properties (via ClientProfile)
        public int? ClientProfileId { get; set; }
        public virtual ClientProfile? ClientProfile { get; set; }

        // PhotoShoot Navigation properties
        public int? PhotoShootId { get; set; }
        public PhotoShoot? PhotoShoot { get; set; }

        // Invoice Items
        public ICollection<InvoiceItem>? InvoiceItems { get; set; }

        // Payments - separate tracking for payment history
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Computed properties
        [NotMapped]
        public decimal TotalAmount => Amount + Tax;

        [NotMapped]
        public decimal AmountPaid => Payments?.Where(p => !p.IsRefund).Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal AmountRefunded => Payments?.Where(p => p.IsRefund).Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal BalanceDue => TotalAmount - AmountPaid + AmountRefunded;
    }
}
