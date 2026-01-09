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
    // COMPLETED: [HIGH-PERF] GetAllGalleriesAsync - now uses SQL-level aggregation
    // COMPLETED: [HIGH-PERF] GetGalleryDetailsAsync - now has pagination support
    // COMPLETED: [HIGH-PERF] GetGalleryStatsAsync - now uses SQL-level counts
    // COMPLETED: [HIGH] GetGalleryAccessUrlAsync - generates gallery-specific URLs with token/slug
    // COMPLETED: [HIGH] GrantAccessAsync - accepts configurable permissions
    // COMPLETED: [FEATURE] Public gallery sharing with token-based access
    // COMPLETED: [FEATURE] Download audit trail logging
    // TODO: [MEDIUM] Add gallery search/filtering functionality
    // TODO: [FEATURE] Add gallery expiry notification emails (requires email service integration)
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
                // Performance optimized: Uses SQL-level aggregation instead of loading all photos into memory
                var galleries = await _context.Galleries
                    .AsNoTracking()
                    .OrderByDescending(g => g.CreatedDate)
                    .Select(g => new GalleryListItemViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        CreatedDate = g.CreatedDate,
                        ExpiryDate = g.ExpiryDate,
                        IsActive = g.IsActive,
                        // SQL-level count - much more efficient than loading photos
                        PhotoCount = g.Albums.Sum(a => a.Photos.Count),
                        SessionCount = g.Sessions.Count,
                        TotalProofs = g.Sessions.SelectMany(s => s.Proofs).Count(),
                        LastAccessDate = g.Sessions.Max(s => (DateTime?)s.LastAccessDate)
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

        public async Task<GalleryDetailsViewModel?> GetGalleryDetailsAsync(int id, int page = 1, int pageSize = 50)
        {
            try
            {
                // First, get gallery metadata without loading all photos
                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gallery == null)
                    return null;

                // Get photo count using SQL aggregation
                var photoCount = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == id))
                    .CountAsync();

                // Get paginated photos - load only what we need
                var photos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == id))
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PhotoViewModel
                    {
                        Id = p.Id,
                        Title = p.Title ?? p.FileName ?? "",
                        ThumbnailPath = p.ThumbnailPath ?? "",
                        FullImagePath = p.FullImagePath ?? ""
                    })
                    .ToListAsync();

                // Get session stats using SQL aggregation
                var sessionStats = await _context.GallerySessions
                    .Where(s => s.GalleryId == id)
                    .GroupBy(s => 1)
                    .Select(g => new
                    {
                        TotalSessions = g.Count(),
                        ActiveSessions = g.Count(s => s.LastAccessDate > DateTime.UtcNow.AddHours(-24)),
                        LastAccessDate = g.Max(s => (DateTime?)s.LastAccessDate)
                    })
                    .FirstOrDefaultAsync();

                // Get recent sessions
                var recentSessions = await _context.GallerySessions
                    .Where(s => s.GalleryId == id)
                    .OrderByDescending(s => s.CreatedDate)
                    .Take(10)
                    .Select(s => new GallerySessionViewModel
                    {
                        Id = s.Id,
                        SessionToken = s.SessionToken,
                        CreatedDate = s.CreatedDate,
                        LastAccessDate = s.LastAccessDate,
                        ProofCount = s.Proofs.Count
                    })
                    .ToListAsync();

                // Get proof stats using SQL aggregation
                var proofStats = await _context.Proofs
                    .Where(p => p.Session.GalleryId == id)
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        TotalProofs = g.Count(),
                        TotalFavorites = g.Count(p => p.IsFavorite),
                        TotalEditingRequests = g.Count(p => p.IsMarkedForEditing)
                    })
                    .FirstOrDefaultAsync();

                var viewModel = new GalleryDetailsViewModel
                {
                    Id = gallery.Id,
                    Name = gallery.Name,
                    Description = gallery.Description,
                    CreatedDate = gallery.CreatedDate,
                    ExpiryDate = gallery.ExpiryDate,
                    IsActive = gallery.IsActive,
                    BrandColor = gallery.BrandColor,
                    PhotoCount = photoCount,
                    Photos = photos,
                    TotalSessions = sessionStats?.TotalSessions ?? 0,
                    ActiveSessions = sessionStats?.ActiveSessions ?? 0,
                    LastAccessDate = sessionStats?.LastAccessDate,
                    RecentSessions = recentSessions,
                    TotalProofs = proofStats?.TotalProofs ?? 0,
                    TotalFavorites = proofStats?.TotalFavorites ?? 0,
                    TotalEditingRequests = proofStats?.TotalEditingRequests ?? 0,
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
                // Create gallery entity
                var gallery = new Gallery
                {
                    Name = model.Name,
                    Description = model.Description,
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

                // Grant access to selected clients
                if (model.SelectedClientProfileIds?.Any() == true)
                {
                    foreach (var clientProfileId in model.SelectedClientProfileIds)
                    {
                        await GrantAccessAsync(gallery.Id, clientProfileId);
                    }
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

        // Grant gallery access to a client with configurable permissions
        public async Task<GalleryAccess> GrantAccessAsync(
            int galleryId,
            int clientProfileId,
            DateTime? expiryDate = null,
            bool canDownload = true,
            bool canProof = true,
            bool canOrder = true)
        {
            try
            {
                // Check if access already exists
                var existingAccess = await _context.GalleryAccesses
                    .FirstOrDefaultAsync(ga => ga.GalleryId == galleryId && ga.ClientProfileId == clientProfileId);

                if (existingAccess != null)
                {
                    // Reactivate and update permissions
                    existingAccess.IsActive = true;
                    existingAccess.ExpiryDate = expiryDate;
                    existingAccess.CanDownload = canDownload;
                    existingAccess.CanProof = canProof;
                    existingAccess.CanOrder = canOrder;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Access updated for gallery {galleryId} to client profile {clientProfileId} " +
                        $"(Download: {canDownload}, Proof: {canProof}, Order: {canOrder})");

                    return existingAccess;
                }

                var access = new GalleryAccess
                {
                    GalleryId = galleryId,
                    ClientProfileId = clientProfileId,
                    GrantedDate = DateTime.UtcNow,
                    ExpiryDate = expiryDate,
                    IsActive = true,
                    CanDownload = canDownload,
                    CanProof = canProof,
                    CanOrder = canOrder
                };

                _context.GalleryAccesses.Add(access);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Access granted for gallery {galleryId} to client profile {clientProfileId} " +
                    $"(Download: {canDownload}, Proof: {canProof}, Order: {canOrder})");

                return access;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error granting access for gallery {galleryId} to client profile {clientProfileId}");
                throw;
            }
        }

        // Revoke gallery access from a client
        public async Task<bool> RevokeAccessAsync(int galleryId, int clientProfileId)
        {
            try
            {
                var access = await _context.GalleryAccesses
                    .FirstOrDefaultAsync(ga => ga.GalleryId == galleryId && ga.ClientProfileId == clientProfileId);

                if (access == null)
                    return false;

                access.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Access revoked for gallery {galleryId} from client profile {clientProfileId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking access for gallery {galleryId} from client profile {clientProfileId}");
                throw;
            }
        }

        // Validate if a user has access to a gallery
        public async Task<bool> ValidateUserAccessAsync(int galleryId, string userId)
        {
            try
            {
                var clientProfile = await _context.ClientProfiles
                    .FirstOrDefaultAsync(cp => cp.UserId == userId);

                if (clientProfile == null)
                    return false;

                return await _context.GalleryAccesses
                    .AnyAsync(ga => ga.GalleryId == galleryId &&
                                    ga.ClientProfileId == clientProfile.Id &&
                                    ga.IsActive &&
                                    (!ga.ExpiryDate.HasValue || ga.ExpiryDate > DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating user access for gallery {galleryId}");
                throw;
            }
        }

        // Get all clients with access to a gallery
        public async Task<IEnumerable<GalleryAccess>> GetGalleryAccessesAsync(int galleryId)
        {
            try
            {
                return await _context.GalleryAccesses
                    .Include(ga => ga.ClientProfile)
                        .ThenInclude(cp => cp.User)
                    .Where(ga => ga.GalleryId == galleryId)
                    .OrderByDescending(ga => ga.GrantedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving gallery accesses for gallery {galleryId}");
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
                    .Include(a => a.ClientProfile)
                        .ThenInclude(cp => cp.User)
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
                        ClientName = a.ClientProfile != null && a.ClientProfile.User != null
                            ? $"{a.ClientProfile.User.FirstName} {a.ClientProfile.User.LastName}"
                            : null,
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

                // Performance optimized: Use SQL-level counts instead of loading entities
                // Single query to get all gallery-related counts
                var galleryStats = await _context.Galleries
                    .GroupBy(g => 1)
                    .Select(g => new
                    {
                        TotalGalleries = g.Count(),
                        ActiveGalleries = g.Count(x => x.IsActive && x.ExpiryDate > now),
                        ExpiredGalleries = g.Count(x => x.ExpiryDate <= now),
                        PublicGalleries = g.Count(x => x.AllowPublicAccess)
                    })
                    .FirstOrDefaultAsync();

                // Count photos using SQL-level aggregation (no Include needed)
                var totalPhotosInGalleries = await _context.Photos
                    .Where(p => p.Album.Galleries.Any())
                    .Select(p => p.Id)
                    .Distinct()
                    .CountAsync();

                var totalSessions = await _context.GallerySessions.CountAsync();

                var stats = new GalleryStatsSummaryViewModel
                {
                    TotalGalleries = galleryStats?.TotalGalleries ?? 0,
                    ActiveGalleries = galleryStats?.ActiveGalleries ?? 0,
                    ExpiredGalleries = galleryStats?.ExpiredGalleries ?? 0,
                    TotalSessions = totalSessions,
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

        public async Task<string> GetGalleryAccessUrlAsync(int galleryId, string baseUrl)
        {
            try
            {
                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == galleryId);

                if (gallery == null)
                    return $"{baseUrl.TrimEnd('/')}/Gallery";

                // Prefer slug for SEO-friendly URLs, fall back to public token, then ID
                if (!string.IsNullOrEmpty(gallery.Slug))
                {
                    return $"{baseUrl.TrimEnd('/')}/gallery/{gallery.Slug}";
                }
                else if (gallery.AllowPublicAccess && !string.IsNullOrEmpty(gallery.PublicAccessToken))
                {
                    return $"{baseUrl.TrimEnd('/')}/gallery/view/{gallery.PublicAccessToken}";
                }
                else
                {
                    // Authenticated access only - use gallery ID
                    return $"{baseUrl.TrimEnd('/')}/Gallery/Details/{galleryId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating access URL for gallery {galleryId}");
                return $"{baseUrl.TrimEnd('/')}/Gallery/Details/{galleryId}";
            }
        }

        #region Public Access (Token-based, no login required)

        /// <summary>
        /// Enable public access for a gallery by generating a unique token
        /// </summary>
        public async Task<string> EnablePublicAccessAsync(int galleryId)
        {
            try
            {
                var gallery = await _context.Galleries.FindAsync(galleryId);
                if (gallery == null)
                    throw new InvalidOperationException($"Gallery not found: {galleryId}");

                // Generate new token if not exists
                if (string.IsNullOrEmpty(gallery.PublicAccessToken))
                {
                    gallery.GeneratePublicAccessToken();
                }

                gallery.AllowPublicAccess = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Public access enabled for gallery {galleryId} with token {gallery.PublicAccessToken}");

                return gallery.PublicAccessToken!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enabling public access for gallery {galleryId}");
                throw;
            }
        }

        /// <summary>
        /// Disable public access for a gallery (keeps token for potential re-enable)
        /// </summary>
        public async Task<bool> DisablePublicAccessAsync(int galleryId)
        {
            try
            {
                var gallery = await _context.Galleries.FindAsync(galleryId);
                if (gallery == null)
                    return false;

                gallery.AllowPublicAccess = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Public access disabled for gallery {galleryId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disabling public access for gallery {galleryId}");
                throw;
            }
        }

        /// <summary>
        /// Get a gallery by its public access token
        /// </summary>
        public async Task<Gallery?> GetGalleryByPublicTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                return await _context.Galleries
                    .Include(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                    .FirstOrDefaultAsync(g =>
                        g.PublicAccessToken == token &&
                        g.AllowPublicAccess &&
                        g.IsActive &&
                        g.ExpiryDate > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving gallery by public token");
                throw;
            }
        }

        /// <summary>
        /// Validate if a public access token is valid for a gallery
        /// </summary>
        public async Task<bool> ValidatePublicAccessAsync(int galleryId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                return await _context.Galleries
                    .AnyAsync(g =>
                        g.Id == galleryId &&
                        g.PublicAccessToken == token &&
                        g.AllowPublicAccess &&
                        g.IsActive &&
                        g.ExpiryDate > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating public access for gallery {galleryId}");
                throw;
            }
        }

        #endregion

        #region Download Audit Logging

        /// <summary>
        /// Log a photo download for audit trail
        /// </summary>
        public async Task LogDownloadAsync(int galleryId, int photoId, string? userId, string? ipAddress)
        {
            try
            {
                // Check if ActivityService is available via DI or use direct logging
                // For now, log to application logger - in production this should go to audit table
                var logMessage = $"Photo download - Gallery: {galleryId}, Photo: {photoId}, " +
                    $"User: {userId ?? "anonymous"}, IP: {ipAddress ?? "unknown"}, Time: {DateTime.UtcNow:O}";

                _logger.LogInformation(logMessage);

                // Update gallery session last access if there's an active session
                if (!string.IsNullOrEmpty(userId))
                {
                    var clientProfile = await _context.ClientProfiles
                        .FirstOrDefaultAsync(cp => cp.UserId == userId);

                    if (clientProfile != null)
                    {
                        var session = await _context.GallerySessions
                            .Where(s => s.GalleryId == galleryId)
                            .OrderByDescending(s => s.LastAccessDate)
                            .FirstOrDefaultAsync();

                        if (session != null)
                        {
                            session.LastAccessDate = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't throw - logging failures shouldn't break downloads
                _logger.LogError(ex, $"Error logging download for gallery {galleryId}, photo {photoId}");
            }
        }

        #endregion
    }
}
