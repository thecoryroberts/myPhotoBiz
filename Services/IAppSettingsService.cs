using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service interface for managing application-wide settings.
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Gets the current application settings.
        /// Creates default settings if none exist.
        /// </summary>
        Task<AppSettings> GetSettingsAsync();

        /// <summary>
        /// Updates the application settings.
        /// </summary>
        Task<AppSettings> UpdateSettingsAsync(AppSettings settings, string? userId = null);

        /// <summary>
        /// Updates a specific section of settings.
        /// </summary>
        Task<AppSettings> UpdateSectionAsync<T>(T sectionData, string? userId = null) where T : class;

        /// <summary>
        /// Saves a logo file and returns the path.
        /// </summary>
        Task<string> SaveLogoAsync(IFormFile file, string logoType);

        /// <summary>
        /// Deletes a logo file.
        /// </summary>
        Task DeleteLogoAsync(string logoType);

        /// <summary>
        /// Resets settings to defaults.
        /// </summary>
        Task<AppSettings> ResetToDefaultsAsync(string? userId = null);

        /// <summary>
        /// Gets cached settings for quick access in views.
        /// </summary>
        AppSettings GetCachedSettings();

        /// <summary>
        /// Clears the settings cache.
        /// </summary>
        void ClearCache();
    }
}
