
using System.ComponentModel.DataAnnotations;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents the photo shoot.
    /// </summary>
    public class PhotoShoot
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Date & Time")]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [Display(Name = "End Date & Time")]
        public DateTime EndTime { get; set; }

        [Range(0, 24, ErrorMessage = "Duration hours must be between 0 and 24")]
        public int DurationHours { get; set; } = 2;

        [Range(0, 59, ErrorMessage = "Duration minutes must be between 0 and 59")]
        public int DurationMinutes { get; set; } = 0;

        [Required]
        public required string Location { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public string? Notes { get; set; }

        public PhotoShootStatus Status { get; set; } = PhotoShootStatus.InProgress;

        /// <summary>
        /// Type/category of photoshoot (Wedding, Portrait, Event, etc.)
        /// </summary>
        [Display(Name = "Shoot Type")]
        public ShootType ShootType { get; set; } = ShootType.Portrait;

        /// <summary>
        /// Soft delete flag to preserve history
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// User ID who created this record
        /// </summary>
        public string? CreatedByUserId { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }

        /// <summary>
        /// User ID who last updated this record
        /// </summary>
        public string? UpdatedByUserId { get; set; }
        public virtual ApplicationUser? UpdatedByUser { get; set; }

        /// <summary>
        /// Indicates if this is a recurring photoshoot
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Recurrence pattern (Monthly, Weekly, etc.) - stored as string for flexibility
        /// </summary>
        [StringLength(50)]
        public string? RecurrencePattern { get; set; }

        /// <summary>
        /// Next date for recurring shoot
        /// </summary>
        public DateTime? NextRecurrenceDate { get; set; }

        // Foreign keys - Client relationship via ClientProfile
        [Required]
        public int ClientProfileId { get; set; }
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        // Photographer assignment - consolidated to single FK via PhotographerProfile
        public int? PhotographerProfileId { get; set; }
        public virtual PhotographerProfile? PhotographerProfile { get; set; }

        // Navigation properties
        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
