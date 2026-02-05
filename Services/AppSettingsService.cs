using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing application-wide settings with caching support.
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AppSettingsService> _logger;
        private const string CacheKey = "AppSettings";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        private static readonly object _lock = new();
        private static AppSettings? _cachedSettings;

        public AppSettingsService(
            ApplicationDbContext context,
            IMemoryCache cache,
            IWebHostEnvironment environment,
            ILogger<AppSettingsService> logger)
        {
            _context = context;
            _cache = cache;
            _environment = environment;
            _logger = logger;
        }

        public async Task<AppSettings> GetSettingsAsync()
        {
            // Try cache first
            if (_cache.TryGetValue(CacheKey, out AppSettings? cached) && cached != null)
            {
                return cached;
            }

            // Get from database
            var settings = await _context.Set<AppSettings>()
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create default settings
                settings = new AppSettings();
                _context.Add(settings);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created default application settings");
            }

            // Cache the settings
            _cache.Set(CacheKey, settings, CacheDuration);
            lock (_lock)
            {
                _cachedSettings = settings;
            }

            return settings;
        }

        public AppSettings GetCachedSettings()
        {
            // Return static cache for synchronous access (e.g., in views)
            lock (_lock)
            {
                if (_cachedSettings != null)
                    return _cachedSettings;
            }

            // Fallback to memory cache or database
            if (_cache.TryGetValue(CacheKey, out AppSettings? cached) && cached != null)
            {
                lock (_lock)
                {
                    _cachedSettings = cached;
                }
                return cached;
            }

            // If no cache, return defaults (async load will populate later)
            var defaults = new AppSettings();
            lock (_lock)
            {
                _cachedSettings = defaults;
            }
            return defaults;
        }

        public async Task<AppSettings> UpdateSettingsAsync(AppSettings settings, string? userId = null)
        {
            var existing = await _context.Set<AppSettings>().FirstOrDefaultAsync();

            if (existing == null)
            {
                settings.CreatedDate = DateTime.UtcNow;
                settings.UpdatedDate = DateTime.UtcNow;
                settings.UpdatedByUserId = userId;
                _context.Add(settings);
            }
            else
            {
                // Update all properties
                _context.Entry(existing).CurrentValues.SetValues(settings);
                existing.UpdatedDate = DateTime.UtcNow;
                existing.UpdatedByUserId = userId;
            }

            await _context.SaveChangesAsync();
            ClearCache();

            _logger.LogInformation("Application settings updated by user {UserId}", userId);

            return await GetSettingsAsync();
        }

        public async Task<AppSettings> UpdateSectionAsync<T>(T sectionData, string? userId = null) where T : class
        {
            var settings = await _context.Set<AppSettings>().FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSettings();
                _context.Add(settings);
            }

            // Copy matching properties from section data
            var sectionProperties = typeof(T).GetProperties();
            var settingsProperties = typeof(AppSettings).GetProperties();

            foreach (var sectionProp in sectionProperties)
            {
                var settingsProp = settingsProperties.FirstOrDefault(p => p.Name == sectionProp.Name);
                if (settingsProp != null && settingsProp.CanWrite)
                {
                    var value = sectionProp.GetValue(sectionData);
                    settingsProp.SetValue(settings, value);
                }
            }

            settings.UpdatedDate = DateTime.UtcNow;
            settings.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
            ClearCache();

            return await GetSettingsAsync();
        }

        public async Task<string> SaveLogoAsync(IFormFile file, string logoType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Allowed: JPG, PNG, GIF, SVG, WebP");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size must be less than 5MB");

            // Create directory if needed
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "branding");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            // Generate filename
            var fileName = $"{logoType}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Delete old logo if exists
            await DeleteLogoAsync(logoType);

            // Save new file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/branding/{fileName}";
            _logger.LogInformation("Saved {LogoType} logo: {Path}", logoType, relativePath);

            return relativePath;
        }

        public async Task DeleteLogoAsync(string logoType)
        {
            var settings = await GetSettingsAsync();
            string? currentPath = logoType switch
            {
                "logo" => settings.LogoPath,
                "logo-dark" => settings.LogoDarkPath,
                "favicon" => settings.FaviconPath,
                _ => null
            };

            if (!string.IsNullOrEmpty(currentPath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, currentPath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted {LogoType} logo: {Path}", logoType, currentPath);
                }
            }
        }

        public async Task<AppSettings> ResetToDefaultsAsync(string? userId = null)
        {
            var existing = await _context.Set<AppSettings>().FirstOrDefaultAsync();

            if (existing != null)
            {
                _context.Remove(existing);
                await _context.SaveChangesAsync();
            }

            ClearCache();

            _logger.LogWarning("Application settings reset to defaults by user {UserId}", userId);

            return await GetSettingsAsync();
        }

        public void ClearCache()
        {
            _cache.Remove(CacheKey);
            lock (_lock)
            {
                _cachedSettings = null;
            }
        }
    }
}
