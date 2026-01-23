using System.ComponentModel.DataAnnotations;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents the client profile.
    /// </summary>
    public class ClientProfile
    {
        public int Id { get; set; }

        // 1:1 relationship with ApplicationUser
        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        // Business-specific data (migrated from Client)
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Client status and categorization
        public ClientStatus Status { get; set; } = ClientStatus.Active;
        public ClientCategory Category { get; set; } = ClientCategory.Regular;

        // Referral tracking
        public ReferralSource ReferralSource { get; set; } = ReferralSource.Unknown;
        [StringLength(200)]
        public string? ReferralDetails { get; set; }  // e.g., "Referred by John Smith" or "Found via Instagram"

        // Contact preferences
        public ContactPreference ContactPreference { get; set; } = ContactPreference.Email;

        // Soft delete support
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }

        // File Manager folder link - stores photos for this client
        public int? FolderId { get; set; }
        public virtual FileItem? Folder { get; set; }

        // Navigation properties (relationships migrated from Client)
        public virtual ICollection<PhotoShoot> PhotoShoots { get; set; } = new List<PhotoShoot>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<ClientBadge> ClientBadges { get; set; } = new List<ClientBadge>();
        public virtual ICollection<GalleryAccess> GalleryAccesses { get; set; } = new List<GalleryAccess>();
        public virtual ICollection<BookingRequest> BookingRequests { get; set; } = new List<BookingRequest>();

        // Computed property for convenience (gets from ApplicationUser)
        public string FullName => User != null ? $"{User.FirstName} {User.LastName}" : string.Empty;
        public string Email => User?.Email ?? string.Empty;

        /// <summary>
        /// Calculate lifetime value based on paid invoices
        /// </summary>
        public decimal LifetimeValue => Invoices?
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Sum(i => i.Amount) ?? 0;

        /// <summary>
        /// Get total number of completed photo shoots
        /// </summary>
        public int CompletedShootsCount => PhotoShoots?
            .Count(ps => ps.Status == PhotoShootStatus.Completed) ?? 0;

        /// <summary>
        /// Check if client has any unpaid invoices
        /// </summary>
        public bool HasUnpaidInvoices => Invoices?
            .Any(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue) ?? false;

        /// <summary>
        /// Check if client has any active bookings or scheduled shoots
        /// </summary>
        public bool HasActiveBookings =>
            (BookingRequests?.Any(br => br.Status == BookingStatus.Pending || br.Status == BookingStatus.Confirmed) ?? false) ||
            (PhotoShoots?.Any(ps => ps.Status == PhotoShootStatus.Scheduled) ?? false);
    }
}
