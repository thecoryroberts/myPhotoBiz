using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Controller for managing application-wide settings.
    /// Admin only access.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly IAppSettingsService _settingsService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            IAppSettingsService settingsService,
            UserManager<ApplicationUser> userManager,
            ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Settings overview/dashboard.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Settings";
            ViewBag.PageTitle = "Application Settings";
            ViewBag.SubTitle = "Dashboard";
            ViewBag.SubTitleUrl = Url.Action("Index", "Dashboard");
            ViewBag.Icon = "ti-settings";
            ViewBag.Color = "primary";
            ViewBag.Purpose = "Configure application settings.";

            var settings = await _settingsService.GetSettingsAsync();
            var vm = new SettingsOverviewViewModel
            {
                Settings = settings,
                LastUpdated = settings.UpdatedDate
            };

            if (!string.IsNullOrEmpty(settings.UpdatedByUserId))
            {
                var user = await _userManager.FindByIdAsync(settings.UpdatedByUserId);
                vm.LastUpdatedBy = user != null ? $"{user.FirstName} {user.LastName}" : null;
            }

            return View(vm);
        }

        #region Business Settings
        public async Task<IActionResult> Business()
        {
            SetViewBag("Business Information", "ti-building");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToBusinessViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Business(BusinessSettingsViewModel model)
        {
            SetViewBag("Business Information", "ti-building");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "Business information updated successfully.";
                return RedirectToAction(nameof(Business));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region Social Media Settings
        public async Task<IActionResult> Social()
        {
            SetViewBag("Social Media", "ti-brand-instagram");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToSocialViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Social(SocialMediaSettingsViewModel model)
        {
            SetViewBag("Social Media", "ti-brand-instagram");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "Social media links updated successfully.";
                return RedirectToAction(nameof(Social));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating social media settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region Branding Settings
        public async Task<IActionResult> Branding()
        {
            SetViewBag("Branding & Colors", "ti-palette");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToBrandingViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Branding(BrandingSettingsViewModel model)
        {
            SetViewBag("Branding & Colors", "ti-palette");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                var settings = await _settingsService.GetSettingsAsync();

                // Handle logo uploads
                if (model.LogoFile != null)
                {
                    settings.LogoPath = await _settingsService.SaveLogoAsync(model.LogoFile, "logo");
                }

                if (model.LogoDarkFile != null)
                {
                    settings.LogoDarkPath = await _settingsService.SaveLogoAsync(model.LogoDarkFile, "logo-dark");
                }

                if (model.FaviconFile != null)
                {
                    settings.FaviconPath = await _settingsService.SaveLogoAsync(model.FaviconFile, "favicon");
                }

                // Update colors
                settings.PrimaryColor = model.PrimaryColor;
                settings.SecondaryColor = model.SecondaryColor;
                settings.AccentColor = model.AccentColor;
                settings.SuccessColor = model.SuccessColor;
                settings.WarningColor = model.WarningColor;
                settings.DangerColor = model.DangerColor;

                await _settingsService.UpdateSettingsAsync(settings, userId);
                TempData["Success"] = "Branding settings updated successfully.";
                return RedirectToAction(nameof(Branding));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branding settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo(string logoType)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var settings = await _settingsService.GetSettingsAsync();

                await _settingsService.DeleteLogoAsync(logoType);

                switch (logoType)
                {
                    case "logo":
                        settings.LogoPath = null;
                        break;
                    case "logo-dark":
                        settings.LogoDarkPath = null;
                        break;
                    case "favicon":
                        settings.FaviconPath = null;
                        break;
                }

                await _settingsService.UpdateSettingsAsync(settings, userId);
                TempData["Success"] = "Logo deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logo");
                TempData["Error"] = "An error occurred while deleting the logo.";
            }

            return RedirectToAction(nameof(Branding));
        }
        #endregion

        #region Invoice Settings
        public async Task<IActionResult> Invoice()
        {
            SetViewBag("Invoice Settings", "ti-file-invoice");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToInvoiceViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invoice(InvoiceSettingsViewModel model)
        {
            SetViewBag("Invoice Settings", "ti-file-invoice");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "Invoice settings updated successfully.";
                return RedirectToAction(nameof(Invoice));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region Booking Settings
        public async Task<IActionResult> Booking()
        {
            SetViewBag("Booking Settings", "ti-calendar");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToBookingViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booking(BookingSettingsViewModel model)
        {
            SetViewBag("Booking Settings", "ti-calendar");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "Booking settings updated successfully.";
                return RedirectToAction(nameof(Booking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region Gallery Settings
        public async Task<IActionResult> Gallery()
        {
            SetViewBag("Gallery Settings", "ti-photo");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToGalleryViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gallery(GallerySettingsViewModel model)
        {
            SetViewBag("Gallery Settings", "ti-photo");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "Gallery settings updated successfully.";
                return RedirectToAction(nameof(Gallery));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating gallery settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region Email Settings
        public async Task<IActionResult> Email()
        {
            SetViewBag("Email Settings", "ti-mail");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToEmailViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Email(EmailSettingsViewModel model)
        {
            SetViewBag("Email Settings", "ti-mail");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "Email settings updated successfully.";
                return RedirectToAction(nameof(Email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region System Settings
        public async Task<IActionResult> System()
        {
            SetViewBag("System Settings", "ti-adjustments");
            var settings = await _settingsService.GetSettingsAsync();
            var vm = MapToSystemViewModel(settings);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> System(SystemSettingsViewModel model)
        {
            SetViewBag("System Settings", "ti-adjustments");

            // Repopulate dropdowns
            model.AvailableTimezones = GetTimezones();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.UpdateSectionAsync(model, userId);
                TempData["Success"] = "System settings updated successfully.";
                return RedirectToAction(nameof(System));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system settings");
                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }
        #endregion

        #region Reset Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _settingsService.ResetToDefaultsAsync(userId);
                TempData["Success"] = "Settings have been reset to defaults.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting settings");
                TempData["Error"] = "An error occurred while resetting settings.";
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Helpers
        private void SetViewBag(string title, string icon)
        {
            ViewBag.Title = title;
            ViewBag.PageTitle = title;
            ViewBag.SubTitle = "Settings";
            ViewBag.SubTitleUrl = Url.Action("Index", "Settings");
            ViewBag.Icon = icon;
            ViewBag.Color = "primary";
            ViewBag.BackUrl = Url.Action("Index", "Settings");
            ViewBag.Purpose = $"Configure {title.ToLower()}.";
        }

        private static BusinessSettingsViewModel MapToBusinessViewModel(AppSettings settings) => new()
        {
            BusinessName = settings.BusinessName,
            Tagline = settings.Tagline,
            BusinessDescription = settings.BusinessDescription,
            BusinessEmail = settings.BusinessEmail,
            BusinessPhone = settings.BusinessPhone,
            BusinessAddress = settings.BusinessAddress,
            WebsiteUrl = settings.WebsiteUrl
        };

        private static SocialMediaSettingsViewModel MapToSocialViewModel(AppSettings settings) => new()
        {
            FacebookUrl = settings.FacebookUrl,
            InstagramUrl = settings.InstagramUrl,
            TwitterUrl = settings.TwitterUrl,
            LinkedInUrl = settings.LinkedInUrl,
            PinterestUrl = settings.PinterestUrl,
            YouTubeUrl = settings.YouTubeUrl
        };

        private static BrandingSettingsViewModel MapToBrandingViewModel(AppSettings settings) => new()
        {
            LogoPath = settings.LogoPath,
            LogoDarkPath = settings.LogoDarkPath,
            FaviconPath = settings.FaviconPath,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            AccentColor = settings.AccentColor,
            SuccessColor = settings.SuccessColor,
            WarningColor = settings.WarningColor,
            DangerColor = settings.DangerColor
        };

        private static InvoiceSettingsViewModel MapToInvoiceViewModel(AppSettings settings) => new()
        {
            InvoiceHeaderColor = settings.InvoiceHeaderColor,
            InvoiceAccentColor = settings.InvoiceAccentColor,
            InvoiceTextColor = settings.InvoiceTextColor,
            InvoiceNumberPrefix = settings.InvoiceNumberPrefix,
            DefaultPaymentTermsDays = settings.DefaultPaymentTermsDays,
            DefaultTaxRate = settings.DefaultTaxRate,
            CurrencyCode = settings.CurrencyCode,
            CurrencySymbol = settings.CurrencySymbol,
            InvoiceFooterText = settings.InvoiceFooterText,
            InvoiceTermsText = settings.InvoiceTermsText,
            ShowLogoOnInvoice = settings.ShowLogoOnInvoice
        };

        private static BookingSettingsViewModel MapToBookingViewModel(AppSettings settings) => new()
        {
            BookingReferencePrefix = settings.BookingReferencePrefix,
            RequireBookingDeposit = settings.RequireBookingDeposit,
            DefaultDepositPercentage = settings.DefaultDepositPercentage,
            AllowSameDayBookings = settings.AllowSameDayBookings,
            MinimumBookingNoticeHours = settings.MinimumBookingNoticeHours
        };

        private static GallerySettingsViewModel MapToGalleryViewModel(AppSettings settings) => new()
        {
            DefaultGalleryExpiryDays = settings.DefaultGalleryExpiryDays,
            AllowGalleryDownloads = settings.AllowGalleryDownloads,
            EnableGalleryWatermarks = settings.EnableGalleryWatermarks,
            DefaultPhotosPerPage = settings.DefaultPhotosPerPage
        };

        private static EmailSettingsViewModel MapToEmailViewModel(AppSettings settings) => new()
        {
            EmailSenderName = settings.EmailSenderName,
            EmailSignature = settings.EmailSignature,
            SendBookingConfirmations = settings.SendBookingConfirmations,
            SendInvoiceReminders = settings.SendInvoiceReminders,
            InvoiceReminderDays = settings.InvoiceReminderDays
        };

        private SystemSettingsViewModel MapToSystemViewModel(AppSettings settings) => new()
        {
            Timezone = settings.Timezone,
            DateFormat = settings.DateFormat,
            TimeFormat = settings.TimeFormat,
            EnableTwoFactorAuth = settings.EnableTwoFactorAuth,
            SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
            AvailableTimezones = GetTimezones()
        };

        private static List<TimezoneOption> GetTimezones()
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new TimezoneOption
                {
                    Id = tz.Id,
                    DisplayName = tz.DisplayName
                })
                .ToList();
        }
        #endregion
    }
}
