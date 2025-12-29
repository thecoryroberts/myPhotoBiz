using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyPhotoBiz.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GalleryService> _logger;

        public GalleryService(ApplicationDbContext context, ILogger<GalleryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<GalleryListItemViewModel>> GetAllGalleriesAsync()
        {
            try
            {
                var galleries = await _context.Galleries
                    .Include(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                    .Include(g => g.Sessions)
                        .ThenInclude(s => s.Proofs)
                    .AsNoTracking()
                    .OrderByDescending(g => g.CreatedDate)
                    .Select(g => new GalleryListItemViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        ClientCode = g.ClientCode,
                        CreatedDate = g.CreatedDate,
                        ExpiryDate = g.ExpiryDate,
                        IsActive = g.IsActive,
                        PhotoCount = g.Albums.SelectMany(a => a.Photos).Count(),
                        SessionCount = g.Sessions.Count,
                        TotalProofs = g.Sessions.Where(s => s.Proofs != null).SelectMany(s => s.Proofs).Count(),
                        LastAccessDate = g.Sessions.Any() ? g.Sessions.Max(s => s.LastAccessDate) : (DateTime?)null
                    })
                    .ToListAsync();

                return galleries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all galleries");
                throw;
            }
        }

        public async Task<GalleryDetailsViewModel?> GetGalleryDetailsAsync(int id)
        {
            try
            {
                var gallery = await _context.Galleries
                    .Include(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                    .Include(g => g.Sessions)
                        .ThenInclude(s => s.Proofs)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gallery == null)
                    return null;

                var allPhotos = gallery.Albums.SelectMany(a => a.Photos).ToList();

                var viewModel = new GalleryDetailsViewModel
                {
                    Id = gallery.Id,
                    Name = gallery.Name,
                    Description = gallery.Description,
                    ClientCode = gallery.ClientCode,
                    CreatedDate = gallery.CreatedDate,
                    ExpiryDate = gallery.ExpiryDate,
                    IsActive = gallery.IsActive,
                    BrandColor = gallery.BrandColor,
                    PhotoCount = allPhotos.Count,
                    Photos = allPhotos.Select(p => new PhotoViewModel
                    {
                        Id = p.Id,
                        Title = p.Title ?? p.FileName ?? "",
                        ThumbnailPath = p.ThumbnailPath ?? "",
                        FullImagePath = p.FullImagePath ?? ""
                    }).ToList(),
                    TotalSessions = gallery.Sessions.Count,
                    ActiveSessions = gallery.Sessions.Count(s => s.LastAccessDate > DateTime.UtcNow.AddHours(-24)),
                    LastAccessDate = gallery.Sessions.Any() ? gallery.Sessions.Max(s => s.LastAccessDate) : (DateTime?)null,
                    RecentSessions = gallery.Sessions
                        .OrderByDescending(s => s.CreatedDate)
                        .Take(10)
                        .Select(s => new GallerySessionViewModel
                        {
                            Id = s.Id,
                            SessionToken = s.SessionToken,
                            CreatedDate = s.CreatedDate,
                            LastAccessDate = s.LastAccessDate,
                            ProofCount = s.Proofs?.Count ?? 0
                        }).ToList(),
                    TotalProofs = gallery.Sessions.Where(s => s.Proofs != null).SelectMany(s => s.Proofs).Count(),
                    TotalFavorites = gallery.Sessions.Where(s => s.Proofs != null).SelectMany(s => s.Proofs).Count(p => p.IsFavorite),
                    TotalEditingRequests = gallery.Sessions.Where(s => s.Proofs != null).SelectMany(s => s.Proofs).Count(p => p.IsMarkedForEditing),
                    AccessUrl = "" // Will be set by controller with base URL
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving gallery details for ID: {id}");
                throw;
            }
        }

        public async Task<Gallery?> GetGalleryByIdAsync(int id)
        {
            try
            {
                return await _context.Galleries
                    .Include(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                    .FirstOrDefaultAsync(g => g.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving gallery by ID: {id}");
                throw;
            }
        }

        public async Task<Gallery> CreateGalleryAsync(CreateGalleryViewModel model)
        {
            try
            {
                // Generate access codes if needed
                string clientCode = model.ClientCode ?? "";
                string clientPassword = model.ClientPassword ?? "";

                if (model.AutoGenerateCode || string.IsNullOrWhiteSpace(clientCode))
                {
                    (clientCode, clientPassword) = await GenerateAccessCodesAsync();
                }

                // Create gallery entity
                var gallery = new Gallery
                {
                    Name = model.Name,
                    Description = model.Description,
                    ClientCode = clientCode,
                    ClientPassword = clientPassword,
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = model.ExpiryDate,
                    IsActive = model.IsActive,
                    BrandColor = model.BrandColor,
                    LogoPath = "" // Default logo path
                };

                _context.Galleries.Add(gallery);
                await _context.SaveChangesAsync();

                // Associate selected albums
                if (model.SelectedAlbumIds.Any())
                {
                    await AddAlbumsToGalleryAsync(gallery.Id, model.SelectedAlbumIds);
                }

                _logger.LogInformation($"Gallery created: {gallery.Name} (ID: {gallery.Id})");

                return gallery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating gallery");
                throw;
            }
        }

        public async Task<Gallery> UpdateGalleryAsync(EditGalleryViewModel model)
        {
            try
            {
                var gallery = await _context.Galleries
                    .Include(g => g.Albums)
                    .FirstOrDefaultAsync(g => g.Id == model.Id);

                if (gallery == null)
                    throw new InvalidOperationException($"Gallery not found: {model.Id}");

                // Update properties
                gallery.Name = model.Name;
                gallery.Description = model.Description;
                gallery.ExpiryDate = model.ExpiryDate;
                gallery.BrandColor = model.BrandColor;
                gallery.IsActive = model.IsActive;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(model.ClientPassword))
                {
                    gallery.ClientPassword = model.ClientPassword;
                }

                // Update album associations
                var currentAlbumIds = gallery.Albums.Select(a => a.Id).ToList();
                var albumsToAdd = model.SelectedAlbumIds.Except(currentAlbumIds).ToList();
                var albumsToRemove = currentAlbumIds.Except(model.SelectedAlbumIds).ToList();

                if (albumsToRemove.Any())
                {
                    var albumsToRemoveEntities = gallery.Albums.Where(a => albumsToRemove.Contains(a.Id)).ToList();
                    foreach (var album in albumsToRemoveEntities)
                    {
                        gallery.Albums.Remove(album);
                    }
                }

                if (albumsToAdd.Any())
                {
                    var newAlbums = await _context.Albums
                        .Where(a => albumsToAdd.Contains(a.Id))
                        .ToListAsync();

                    foreach (var album in newAlbums)
                    {
                        gallery.Albums.Add(album);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery updated: {gallery.Name} (ID: {gallery.Id})");

                return gallery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating gallery ID: {model.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteGalleryAsync(int id)
        {
            try
            {
                var gallery = await _context.Galleries
                    .Include(g => g.Sessions)
                    .Include(g => g.Albums)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gallery == null)
                    return false;

                // Remove gallery (this will cascade delete sessions if configured)
                _context.Galleries.Remove(gallery);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery deleted: {gallery.Name} (ID: {id})");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting gallery ID: {id}");
                throw;
            }
        }

        public async Task<bool> ToggleGalleryStatusAsync(int id, bool isActive)
        {
            try
            {
                var gallery = await _context.Galleries.FindAsync(id);

                if (gallery == null)
                    return false;

                gallery.IsActive = isActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery {(isActive ? "activated" : "deactivated")}: {gallery.Name} (ID: {id})");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling gallery status for ID: {id}");
                throw;
            }
        }

        public async Task<(string clientCode, string clientPassword)> GenerateAccessCodesAsync()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude similar looking characters
            const int codeLength = 8;
            const int passwordLength = 12;

            string clientCode;
            string clientPassword;

            // Generate unique client code
            do
            {
                clientCode = GenerateRandomString(chars, codeLength);
            }
            while (await _context.Galleries.AnyAsync(g => g.ClientCode == clientCode));

            // Generate password
            clientPassword = GenerateRandomString(chars + "abcdefghijklmnopqrstuvwxyz!@#$%", passwordLength);

            return (clientCode, clientPassword);
        }

        private static string GenerateRandomString(string chars, int length)
        {
            var stringBuilder = new StringBuilder(length);
            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            foreach (var b in randomBytes)
            {
                stringBuilder.Append(chars[b % chars.Length]);
            }

            return stringBuilder.ToString();
        }

        public async Task<(string clientCode, string clientPassword)> RegenerateAccessCodesAsync(int galleryId)
        {
            try
            {
                var gallery = await _context.Galleries.FindAsync(galleryId);

                if (gallery == null)
                    throw new InvalidOperationException($"Gallery not found: {galleryId}");

                var (newCode, newPassword) = await GenerateAccessCodesAsync();

                gallery.ClientCode = newCode;
                gallery.ClientPassword = newPassword;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Access codes regenerated for gallery: {gallery.Name} (ID: {galleryId})");

                return (newCode, newPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error regenerating access codes for gallery ID: {galleryId}");
                throw;
            }
        }

        public async Task<bool> ValidateAccessCodesAsync(string clientCode, string clientPassword)
        {
            try
            {
                return await _context.Galleries
                    .AnyAsync(g => g.ClientCode == clientCode &&
                                   g.ClientPassword == clientPassword &&
                                   g.IsActive &&
                                   g.ExpiryDate > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating access codes");
                throw;
            }
        }

        public async Task<bool> AddAlbumsToGalleryAsync(int galleryId, List<int> albumIds)
        {
            try
            {
                var gallery = await _context.Galleries
                    .Include(g => g.Albums)
                    .FirstOrDefaultAsync(g => g.Id == galleryId);

                if (gallery == null)
                    return false;

                var albums = await _context.Albums
                    .Where(a => albumIds.Contains(a.Id))
                    .ToListAsync();

                foreach (var album in albums)
                {
                    if (!gallery.Albums.Contains(album))
                    {
                        gallery.Albums.Add(album);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Added {albums.Count} albums to gallery ID: {galleryId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding albums to gallery ID: {galleryId}");
                throw;
            }
        }

        public async Task<bool> RemoveAlbumsFromGalleryAsync(int galleryId, List<int> albumIds)
        {
            try
            {
                var gallery = await _context.Galleries
                    .Include(g => g.Albums)
                    .FirstOrDefaultAsync(g => g.Id == galleryId);

                if (gallery == null)
                    return false;

                var albumsToRemove = gallery.Albums.Where(a => albumIds.Contains(a.Id)).ToList();

                foreach (var album in albumsToRemove)
                {
                    gallery.Albums.Remove(album);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Removed {albumsToRemove.Count} albums from gallery ID: {galleryId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing albums from gallery ID: {galleryId}");
                throw;
            }
        }

        public async Task<List<AlbumSelectionViewModel>> GetAvailableAlbumsAsync(int? currentGalleryId = null)
        {
            try
            {
                var query = _context.Albums
                    .Include(a => a.Photos)
                    .Include(a => a.Client)
                    .Include(a => a.Galleries)
                    .AsQueryable();

                var albums = await query
                    .OrderBy(a => a.CreatedDate)
                    .ThenBy(a => a.Name)
                    .Select(a => new AlbumSelectionViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        PhotoCount = a.Photos.Count,
                        ClientName = a.Client != null ? $"{a.Client.FirstName} {a.Client.LastName}" : null,
                        IsSelected = currentGalleryId.HasValue && a.Galleries.Any(g => g.Id == currentGalleryId.Value)
                    })
                    .ToListAsync();

                return albums;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available albums");
                throw;
            }
        }

        public async Task<IEnumerable<GallerySessionViewModel>> GetGallerySessionsAsync(int galleryId)
        {
            try
            {
                var sessions = await _context.GallerySessions
                    .Where(s => s.GalleryId == galleryId)
                    .Include(s => s.Proofs)
                    .AsNoTracking()
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new GallerySessionViewModel
                    {
                        Id = s.Id,
                        SessionToken = s.SessionToken,
                        CreatedDate = s.CreatedDate,
                        LastAccessDate = s.LastAccessDate,
                        ProofCount = s.Proofs != null ? s.Proofs.Count : 0
                    })
                    .ToListAsync();

                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving sessions for gallery ID: {galleryId}");
                throw;
            }
        }

        public async Task<bool> EndSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.GallerySessions.FindAsync(sessionId);

                if (session == null)
                    return false;

                _context.GallerySessions.Remove(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Session ended: {session.SessionToken}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ending session ID: {sessionId}");
                throw;
            }
        }

        public async Task<bool> EndAllSessionsAsync(int galleryId)
        {
            try
            {
                var sessions = await _context.GallerySessions
                    .Where(s => s.GalleryId == galleryId)
                    .ToListAsync();

                _context.GallerySessions.RemoveRange(sessions);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"All sessions ended for gallery ID: {galleryId} ({sessions.Count} sessions)");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ending all sessions for gallery ID: {galleryId}");
                throw;
            }
        }

        public async Task<GalleryStatsSummaryViewModel> GetGalleryStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;

                // Count total photos across all galleries
                var totalPhotosInGalleries = await _context.Galleries
                    .Include(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                    .SelectMany(g => g.Albums.SelectMany(a => a.Photos))
                    .Distinct()
                    .CountAsync();

                var stats = new GalleryStatsSummaryViewModel
                {
                    TotalGalleries = await _context.Galleries.CountAsync(),
                    ActiveGalleries = await _context.Galleries.CountAsync(g => g.IsActive && g.ExpiryDate > now),
                    ExpiredGalleries = await _context.Galleries.CountAsync(g => g.ExpiryDate <= now),
                    TotalSessions = await _context.GallerySessions.CountAsync(),
                    TotalPhotos = totalPhotosInGalleries
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving gallery statistics");
                throw;
            }
        }

        public Task<string> GetGalleryAccessUrlAsync(int galleryId, string baseUrl)
        {
            var url = $"{baseUrl.TrimEnd('/')}/Gallery/Index";
            return Task.FromResult(url);
        }
    }
}
