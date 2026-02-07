using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// View model for the main settings overview page.
    /// </summary>
    public class SettingsOverviewViewModel
    {
        public AppSettings Settings { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string? LastUpdatedBy { get; set; }
    }

    /// <summary>
    /// View model for business information settings.
    /// </summary>
    public class BusinessSettingsViewModel
    {
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
    }

    /// <summary>
    /// View model for social media settings.
    /// </summary>
    public class SocialMediaSettingsViewModel
    {
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
    }

    /// <summary>
    /// View model for branding settings (logos and colors).
    /// </summary>
    public class BrandingSettingsViewModel
    {
        // Current logos
        public string? LogoPath { get; set; }
        public string? LogoDarkPath { get; set; }
        public string? FaviconPath { get; set; }

        // File uploads
        [Display(Name = "Logo (Light Background)")]
        public IFormFile? LogoFile { get; set; }

        [Display(Name = "Logo (Dark Background)")]
        public IFormFile? LogoDarkFile { get; set; }

        [Display(Name = "Favicon")]
        public IFormFile? FaviconFile { get; set; }

        // Colors
        [Required]
        [StringLength(7)]
        [Display(Name = "Primary Color")]
        public string PrimaryColor { get; set; } = "#3b82f6";

        [Required]
        [StringLength(7)]
        [Display(Name = "Secondary Color")]
        public string SecondaryColor { get; set; } = "#64748b";

        [Required]
        [StringLength(7)]
        [Display(Name = "Accent Color")]
        public string AccentColor { get; set; } = "#10b981";

        [Required]
        [StringLength(7)]
        [Display(Name = "Success Color")]
        public string SuccessColor { get; set; } = "#22c55e";

        [Required]
        [StringLength(7)]
        [Display(Name = "Warning Color")]
        public string WarningColor { get; set; } = "#f59e0b";

        [Required]
        [StringLength(7)]
        [Display(Name = "Danger Color")]
        public string DangerColor { get; set; } = "#ef4444";

        // WCAG Contrast Validation Results
        public ColorContrastValidationResult? ContrastValidation { get; set; }

        // Flag to force save despite warnings (user confirmed)
        public bool ConfirmAccessibilityWarnings { get; set; }
    }

    /// <summary>
    /// View model for invoice settings.
    /// </summary>
    public class InvoiceSettingsViewModel
    {
        [Required]
        [StringLength(7)]
        [Display(Name = "Header Color")]
        public string InvoiceHeaderColor { get; set; } = "#3b82f6";

        [Required]
        [StringLength(7)]
        [Display(Name = "Accent Color")]
        public string InvoiceAccentColor { get; set; } = "#1e40af";

        [Required]
        [StringLength(7)]
        [Display(Name = "Text Color")]
        public string InvoiceTextColor { get; set; } = "#1f2937";

        [Required]
        [StringLength(100)]
        [Display(Name = "Invoice Number Prefix")]
        public string InvoiceNumberPrefix { get; set; } = "INV-";

        [Required]
        [Display(Name = "Default Payment Terms (Days)")]
        [Range(1, 365)]
        public int DefaultPaymentTermsDays { get; set; } = 30;

        [Display(Name = "Default Tax Rate (%)")]
        [Range(0, 100)]
        public decimal DefaultTaxRate { get; set; } = 0;

        [Required]
        [StringLength(50)]
        [Display(Name = "Currency Code")]
        public string CurrencyCode { get; set; } = "USD";

        [Required]
        [StringLength(5)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "$";

        [StringLength(2000)]
        [Display(Name = "Invoice Footer Text")]
        public string? InvoiceFooterText { get; set; }

        [StringLength(2000)]
        [Display(Name = "Terms & Conditions")]
        public string? InvoiceTermsText { get; set; }

        [Display(Name = "Show Logo on Invoices")]
        public bool ShowLogoOnInvoice { get; set; } = true;

        // Digital Signature
        public string? SignaturePath { get; set; }

        [Display(Name = "Digital Signature")]
        public IFormFile? SignatureFile { get; set; }

        [Display(Name = "Show Signature on Invoices")]
        public bool ShowSignatureOnInvoice { get; set; } = true;
    }

    /// <summary>
    /// View model for booking settings.
    /// </summary>
    public class BookingSettingsViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Booking Reference Prefix")]
        public string BookingReferencePrefix { get; set; } = "BK-";

        [Display(Name = "Require Deposit for Bookings")]
        public bool RequireBookingDeposit { get; set; } = false;

        [Display(Name = "Default Deposit Percentage")]
        [Range(0, 100)]
        public decimal DefaultDepositPercentage { get; set; } = 25;

        [Display(Name = "Allow Same-Day Bookings")]
        public bool AllowSameDayBookings { get; set; } = false;

        [Display(Name = "Minimum Booking Notice (Hours)")]
        [Range(0, 720)]
        public int MinimumBookingNoticeHours { get; set; } = 24;
    }

    /// <summary>
    /// View model for gallery settings.
    /// </summary>
    public class GallerySettingsViewModel
    {
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
    }

    /// <summary>
    /// View model for email settings.
    /// </summary>
    public class EmailSettingsViewModel
    {
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
    }

    /// <summary>
    /// View model for system settings.
    /// </summary>
    public class SystemSettingsViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Timezone")]
        public string Timezone { get; set; } = "America/New_York";

        [Required]
        [StringLength(20)]
        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "MM/dd/yyyy";

        [Required]
        [StringLength(10)]
        [Display(Name = "Time Format")]
        public string TimeFormat { get; set; } = "h:mm tt";

        [Display(Name = "Enable Two-Factor Authentication")]
        public bool EnableTwoFactorAuth { get; set; } = false;

        [Display(Name = "Session Timeout (Minutes)")]
        [Range(5, 480)]
        public int SessionTimeoutMinutes { get; set; } = 60;

        // Available options for dropdowns
        public List<TimezoneOption> AvailableTimezones { get; set; } = new();
        public List<string> AvailableDateFormats { get; set; } = new()
        {
            "MM/dd/yyyy",
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "MMMM dd, yyyy",
            "dd MMMM yyyy"
        };
        public List<string> AvailableTimeFormats { get; set; } = new()
        {
            "h:mm tt",
            "hh:mm tt",
            "HH:mm",
            "H:mm"
        };
    }

    public class TimezoneOption
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
