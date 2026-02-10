using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Stores application-wide settings for branding, invoices, and business customization.
    /// Only one record should exist in this table (singleton pattern).
    /// </summary>
    public class AppSettings
    {
        public int Id { get; set; }

        #region Business Information
        [Required]
        [StringLength(100)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = "My Photography Business";

        [StringLength(200)]
        [Display(Name = "Tagline")]
        public string? Tagline { get; set; }

        [StringLength(500)]
        [Display(Name = "Business Description")]
        public string? BusinessDescription { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Business Email")]
        public string? BusinessEmail { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Business Phone")]
        public string? BusinessPhone { get; set; }

        [StringLength(500)]
        [Display(Name = "Business Address")]
        public string? BusinessAddress { get; set; }

        [StringLength(200)]
        [Url]
        [Display(Name = "Website URL")]
        public string? WebsiteUrl { get; set; }
        #endregion

        #region Social Media
        [StringLength(200)]
        [Url]
        [Display(Name = "Facebook URL")]
        public string? FacebookUrl { get; set; }

        [StringLength(200)]
        [Url]
        [Display(Name = "Instagram URL")]
        public string? InstagramUrl { get; set; }

        [StringLength(200)]
        [Url]
        [Display(Name = "Twitter/X URL")]
        public string? TwitterUrl { get; set; }

        [StringLength(200)]
        [Url]
        [Display(Name = "LinkedIn URL")]
        public string? LinkedInUrl { get; set; }

        [StringLength(200)]
        [Url]
        [Display(Name = "Pinterest URL")]
        public string? PinterestUrl { get; set; }

        [StringLength(200)]
        [Url]
        [Display(Name = "YouTube URL")]
        public string? YouTubeUrl { get; set; }
        #endregion

        #region Branding - Logos
        [StringLength(500)]
        [Display(Name = "Logo (Light Background)")]
        public string? LogoPath { get; set; }

        [StringLength(500)]
        [Display(Name = "Logo (Dark Background)")]
        public string? LogoDarkPath { get; set; }

        [StringLength(500)]
        [Display(Name = "Favicon")]
        public string? FaviconPath { get; set; }
        #endregion

        #region Branding - Colors
        [StringLength(7)]
        [Display(Name = "Primary Color")]
        public string PrimaryColor { get; set; } = "#3b82f6"; // Blue

        [StringLength(7)]
        [Display(Name = "Secondary Color")]
        public string SecondaryColor { get; set; } = "#64748b"; // Slate

        [StringLength(7)]
        [Display(Name = "Accent Color")]
        public string AccentColor { get; set; } = "#10b981"; // Emerald

        [StringLength(7)]
        [Display(Name = "Success Color")]
        public string SuccessColor { get; set; } = "#22c55e"; // Green

        [StringLength(7)]
        [Display(Name = "Warning Color")]
        public string WarningColor { get; set; } = "#f59e0b"; // Amber

        [StringLength(7)]
        [Display(Name = "Danger Color")]
        public string DangerColor { get; set; } = "#ef4444"; // Red
        #endregion

        #region Invoice Settings
        [StringLength(7)]
        [Display(Name = "Invoice Header Color")]
        public string InvoiceHeaderColor { get; set; } = "#3b82f6";

        [StringLength(7)]
        [Display(Name = "Invoice Accent Color")]
        public string InvoiceAccentColor { get; set; } = "#1e40af";

        [StringLength(7)]
        [Display(Name = "Invoice Text Color")]
        public string InvoiceTextColor { get; set; } = "#1f2937";

        [StringLength(100)]
        [Display(Name = "Invoice Number Prefix")]
        public string InvoiceNumberPrefix { get; set; } = "INV-";

        [Display(Name = "Default Payment Terms (Days)")]
        [Range(1, 365)]
        public int DefaultPaymentTermsDays { get; set; } = 30;

        [Display(Name = "Default Tax Rate (%)")]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DefaultTaxRate { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Currency Code")]
        public string CurrencyCode { get; set; } = "USD";

        [StringLength(5)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "$";

        [StringLength(2000)]
        [Display(Name = "Invoice Footer Text")]
        public string? InvoiceFooterText { get; set; }

        [StringLength(2000)]
        [Display(Name = "Invoice Terms & Conditions")]
        public string? InvoiceTermsText { get; set; }

        [Display(Name = "Show Logo on Invoices")]
        public bool ShowLogoOnInvoice { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Digital Signature")]
        public string? SignaturePath { get; set; }

        [Display(Name = "Show Signature on Invoices")]
        public bool ShowSignatureOnInvoice { get; set; } = true;
        #endregion

        #region Contract Settings
        [StringLength(7)]
        [Display(Name = "Contract Header Color")]
        public string ContractHeaderColor { get; set; } = "#1e40af";

        [StringLength(100)]
        [Display(Name = "Contract Number Prefix")]
        public string ContractNumberPrefix { get; set; } = "CTR-";

        [Display(Name = "Show Logo on Contracts")]
        public bool ShowLogoOnContract { get; set; } = true;
        #endregion

        #region Booking Settings
        [StringLength(100)]
        [Display(Name = "Booking Reference Prefix")]
        public string BookingReferencePrefix { get; set; } = "BK-";

        [Display(Name = "Require Deposit for Bookings")]
        public bool RequireBookingDeposit { get; set; } = false;

        [Display(Name = "Default Deposit Percentage")]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DefaultDepositPercentage { get; set; } = 25;

        [Display(Name = "Allow Same-Day Bookings")]
        public bool AllowSameDayBookings { get; set; } = false;

        [Display(Name = "Minimum Booking Notice (Hours)")]
        [Range(0, 720)]
        public int MinimumBookingNoticeHours { get; set; } = 24;
        #endregion

        #region Gallery Settings
        [Display(Name = "Default Gallery Expiry (Days)")]
        [Range(1, 365)]
        public int DefaultGalleryExpiryDays { get; set; } = 90;

        [Display(Name = "Allow Gallery Downloads")]
        public bool AllowGalleryDownloads { get; set; } = true;

        [Display(Name = "Enable Gallery Watermarks")]
        public bool EnableGalleryWatermarks { get; set; } = true;

        [Display(Name = "Default Photos Per Page")]
        [Range(10, 100)]
        public int DefaultPhotosPerPage { get; set; } = 24;
        #endregion

        #region Email Settings
        [StringLength(200)]
        [Display(Name = "Email Sender Name")]
        public string? EmailSenderName { get; set; }

        [StringLength(500)]
        [Display(Name = "Email Signature")]
        public string? EmailSignature { get; set; }

        [Display(Name = "Send Booking Confirmations")]
        public bool SendBookingConfirmations { get; set; } = true;

        [Display(Name = "Send Invoice Reminders")]
        public bool SendInvoiceReminders { get; set; } = true;

        [Display(Name = "Days Before Due Date to Send Reminder")]
        [Range(1, 30)]
        public int InvoiceReminderDays { get; set; } = 7;
        #endregion

        #region System Settings
        [StringLength(50)]
        [Display(Name = "Timezone")]
        public string Timezone { get; set; } = "America/New_York";

        [StringLength(20)]
        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "MM/dd/yyyy";

        [StringLength(10)]
        [Display(Name = "Time Format")]
        public string TimeFormat { get; set; } = "h:mm tt";

        [Display(Name = "Enable Two-Factor Authentication")]
        public bool EnableTwoFactorAuth { get; set; } = false;

        [Display(Name = "Session Timeout (Minutes)")]
        [Range(5, 480)]
        public int SessionTimeoutMinutes { get; set; } = 60;
        #endregion

        #region Metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public virtual ApplicationUser? UpdatedByUser { get; set; }
        #endregion
    }
}
