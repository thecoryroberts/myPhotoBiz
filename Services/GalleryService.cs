using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing photo galleries and client access.
    /// Features: SQL-level aggregation for performance, pagination support, public gallery sharing
    /// with token-based access, configurable permissions, and download audit trail logging.
    /// </summary>
    public class GalleryService : IGalleryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GalleryService> _logger;
        private readonly IWatermarkService _watermarkService;

        public GalleryService(ApplicationDbContext context, ILogger<GalleryService> logger, IWatermarkService watermarkService)
        {
            _context = context;
            _logger = logger;
            _watermarkService = watermarkService;
        }

        public async Task<IEnumerable<GalleryListItemViewModel>> GetAllGalleriesAsync()
        {
            try
            {
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
                        PhotoCount = g.Albums.SelectMany(a => a.Photos).Count(),
                        SessionCount = g.Sessions.Count,
                        TotalProofs = g.Sessions
                            .SelectMany(s => s.Proofs!)
                            .Count(),
                        LastAccessDate = g.Sessions.Any()
                            ? g.Sessions.Max(s => (DateTime?)s.LastAccessDate)
                            : null,
                        ThumbnailUrl = g.Albums
                            .SelectMany(a => a.Photos)
                            .OrderBy(p => p.DisplayOrder)
                            .Select(p => p.ThumbnailPath)
                            .FirstOrDefault(),
                        WatermarkEnabled = g.WatermarkEnabled
                    })
                    .ToListAsync();

                return galleries ?? Enumerable.Empty<GalleryListItemViewModel>();
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

                // Get photo count using SQL aggregation (guard Album navigation)
                var photoCount = await _context.Photos
                    .Where(p => p.Album != null && p.Album.Galleries.Any(g => g.Id == id))
                    .CountAsync();

                // Get paginated photos - load only what we need
                var photos = await _context.Photos
                    .Where(p => p.Album != null && p.Album.Galleries.Any(g => g.Id == id))
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
                        ProofCount = s.Proofs != null ? s.Proofs.Count : 0
                    })
                    .ToListAsync();

                // Get proof stats using SQL aggregation
                var proofStats = await _context.Proofs
                    .Where(p => p.Session != null && p.Session.GalleryId == id)
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

                // Update watermark settings
                gallery.WatermarkEnabled = model.WatermarkEnabled;
                gallery.WatermarkText = model.WatermarkText;
                gallery.WatermarkImagePath = model.WatermarkImagePath;
                gallery.WatermarkOpacity = model.WatermarkOpacity;
                gallery.WatermarkPosition = model.WatermarkPosition;
                gallery.WatermarkTiled = model.WatermarkTiled;

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
                if (await IsUserStaffAsync(userId))
                    return true;

                // For non-admin users, check ClientProfile access
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

        /// <summary>
        /// Get all available clients for gallery access
        /// </summary>
        public async Task<List<ClientSelectionViewModel>> GetAvailableClientsAsync()
        {
            try
            {
                var clients = await _context.ClientProfiles
                    .Include(static cp => cp.User)
                    .Where(static cp => !cp.IsDeleted && cp.User != null)
                    .OrderBy(static cp => cp.User.LastName)
                    .ThenBy(static cp => cp.User.FirstName)
                    .Select(static cp => new ClientSelectionViewModel
                    {
                        Id = cp.Id,
                        FullName = cp.User != null ? $"{cp.User.FirstName} {cp.User.LastName}" : "Unknown",
                        Email = cp.User != null ? cp.User.Email : null,
                        PhoneNumber = cp.PhoneNumber,
                        IsSelected = false
                    })
                    .ToListAsync();

                return clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available clients");
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

                var totalGalleries = await _context.Galleries.CountAsync();
                var activeGalleries = await _context.Galleries
                    .CountAsync(g => g.IsActive && g.ExpiryDate > now);
                var expiredGalleries = await _context.Galleries
                    .CountAsync(g => g.ExpiryDate <= now);
                var totalSessions = await _context.GallerySessions.CountAsync();
                var totalPhotos = await _context.Photos
                    .Where(p => p.Album != null && p.Album.Galleries.Any())
                    .CountAsync();

                return new GalleryStatsSummaryViewModel
                {
                    TotalGalleries = totalGalleries,
                    ActiveGalleries = activeGalleries,
                    ExpiredGalleries = expiredGalleries,
                    TotalSessions = totalSessions,
                    TotalPhotos = totalPhotos
                };
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

                // Prefer slug for SEO-friendly URLs, fall back to public token, then authenticated ID
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
                    // Authenticated access only - use ViewGallery action
                    return $"{baseUrl.TrimEnd('/')}/Gallery/ViewGallery/{galleryId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating access URL for gallery {galleryId}");
                return $"{baseUrl.TrimEnd('/')}/Gallery/ViewGallery/{galleryId}";
            }
        }

        #region Client Gallery Views

        public async Task<ClientGalleryIndexResult> GetAccessibleGalleriesForUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new ClientGalleryIndexResult { HasProfile = false };
                }

                var roles = await GetUserRolesAsync(userId);
                if (IsStaffRole(roles))
                {
                    var galleries = await _context.Galleries
                        .AsNoTracking()
                        .Where(g => g.IsActive && g.ExpiryDate > DateTime.UtcNow)
                        .OrderByDescending(g => g.CreatedDate)
                        .Select(g => new ClientGalleryViewModel
                        {
                            GalleryId = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            BrandColor = g.BrandColor,
                            PhotoCount = g.Albums.SelectMany(a => a.Photos).Count(),
                            ExpiryDate = g.ExpiryDate,
                            GrantedDate = g.CreatedDate,
                            CanDownload = true,
                            CanProof = true,
                            CanOrder = true,
                            ThumbnailUrl = g.Albums
                                .SelectMany(a => a.Photos)
                                .OrderBy(p => p.DisplayOrder)
                                .Select(p => p.ThumbnailPath)
                                .FirstOrDefault()
                        })
                        .ToListAsync();

                    return new ClientGalleryIndexResult
                    {
                        HasProfile = true,
                        Galleries = galleries
                    };
                }

                var clientProfile = await _context.ClientProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cp => cp.UserId == userId);

                if (clientProfile == null)
                {
                    return new ClientGalleryIndexResult { HasProfile = false };
                }

                var accessibleGalleries = await _context.GalleryAccesses
                    .AsNoTracking()
                    .Where(ga => ga.ClientProfileId == clientProfile.Id &&
                                 ga.IsActive &&
                                 (!ga.ExpiryDate.HasValue || ga.ExpiryDate > DateTime.UtcNow) &&
                                 ga.Gallery.IsActive &&
                                 ga.Gallery.ExpiryDate > DateTime.UtcNow)
                    .OrderByDescending(ga => ga.Gallery.CreatedDate)
                    .Select(ga => new ClientGalleryViewModel
                    {
                        GalleryId = ga.Gallery.Id,
                        Name = ga.Gallery.Name,
                        Description = ga.Gallery.Description,
                        BrandColor = ga.Gallery.BrandColor,
                        PhotoCount = ga.Gallery.Albums.SelectMany(a => a.Photos).Count(),
                        ExpiryDate = ga.Gallery.ExpiryDate,
                        GrantedDate = ga.GrantedDate,
                        CanDownload = ga.CanDownload,
                        CanProof = ga.CanProof,
                        CanOrder = ga.CanOrder,
                        ThumbnailUrl = ga.Gallery.Albums
                            .SelectMany(a => a.Photos)
                            .OrderBy(p => p.DisplayOrder)
                            .Select(p => p.ThumbnailPath)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return new ClientGalleryIndexResult
                {
                    HasProfile = true,
                    Galleries = accessibleGalleries
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving accessible galleries for user {userId}");
                throw;
            }
        }

        public async Task<GalleryViewPageResult?> GetGalleryViewPageForUserAsync(int galleryId, string userId, int page, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                var hasAccess = await ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                    return null;

                var gallery = await GetActiveGalleryAsync(galleryId);
                if (gallery == null)
                    return null;

                var sessionToken = await GetOrCreateUserSessionTokenAsync(galleryId, userId);
                var photoPage = await GetGalleryPhotosPageInternalAsync(galleryId, page, pageSize);
                if (photoPage == null)
                    return null;

                return BuildGalleryViewPageResult(gallery, photoPage, sessionToken, isPublicAccess: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading gallery view page for gallery {galleryId}");
                throw;
            }
        }

        public async Task<GalleryViewPageResult?> GetPublicGalleryViewPageByTokenAsync(string token, int page, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var gallery = await GetPublicGalleryByTokenAsync(token);
                if (gallery == null)
                    return null;

                var sessionToken = await CreateAnonymousSessionAsync(gallery.Id);
                var photoPage = await GetGalleryPhotosPageInternalAsync(gallery.Id, page, pageSize);
                if (photoPage == null)
                    return null;

                return BuildGalleryViewPageResult(gallery, photoPage, sessionToken, isPublicAccess: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public gallery view by token");
                throw;
            }
        }

        public async Task<GalleryViewPageResult?> GetPublicGalleryViewPageBySlugAsync(string slug, int page, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                    return null;

                var gallery = await GetPublicGalleryBySlugAsync(slug);
                if (gallery == null)
                    return null;

                var sessionToken = await CreateAnonymousSessionAsync(gallery.Id);
                var photoPage = await GetGalleryPhotosPageInternalAsync(gallery.Id, page, pageSize);
                if (photoPage == null)
                    return null;

                return BuildGalleryViewPageResult(gallery, photoPage, sessionToken, isPublicAccess: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public gallery view by slug");
                throw;
            }
        }

        public async Task<GalleryPhotosPageResult?> GetGalleryPhotosPageAsync(int galleryId, int page, int pageSize)
        {
            try
            {
                return await GetGalleryPhotosPageInternalAsync(galleryId, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading gallery photos for gallery {galleryId}");
                throw;
            }
        }

        public async Task<GallerySessionInfoResult?> GetGallerySessionInfoAsync(int galleryId, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                var hasAccess = await ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                    return null;

                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == galleryId);

                if (gallery == null || !gallery.IsActive || gallery.ExpiryDate < DateTime.UtcNow)
                    return null;

                var session = await _context.GallerySessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.GalleryId == galleryId && s.UserId == userId);

                return new GallerySessionInfoResult
                {
                    Gallery = gallery,
                    Session = session
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving session info for gallery {galleryId}");
                throw;
            }
        }

        public async Task<bool> EndGallerySessionAsync(int galleryId, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var session = await _context.GallerySessions
                    .FirstOrDefaultAsync(s => s.GalleryId == galleryId && s.UserId == userId);

                if (session == null)
                    return false;

                _context.GallerySessions.Remove(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery session ended for user {userId} on gallery {galleryId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending gallery session");
                throw;
            }
        }

        public async Task<GalleryPhotoDownloadResult> GetPhotoDownloadAsync(int galleryId, int photoId, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.Unauthorized };
                }

                var hasAccess = await ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.Unauthorized };
                }

                if (!await CanUserDownloadAsync(galleryId, userId))
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.Forbidden };
                }

                var photo = await _context.Photos
                    .Include(p => p.Album)
                        .ThenInclude(a => a.Galleries)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == photoId && p.Album.Galleries.Any(g => g.Id == galleryId));

                if (photo == null)
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.NotFound };
                }

                if (string.IsNullOrEmpty(photo.FullImagePath))
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.NotFound };
                }

                var filePath = FileSecurityHelper.GetSafeWwwrootPath(photo.FullImagePath, _logger);
                if (filePath == null)
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.Unauthorized };
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.NotFound };
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = string.IsNullOrEmpty(photo.Title) ? $"photo_{photo.Id}.jpg" : $"{photo.Title}.jpg";

                var gallery = await _context.Galleries.FindAsync(galleryId);
                if (gallery != null && gallery.WatermarkEnabled)
                {
                    var watermarkSettings = CreateWatermarkSettings(gallery);
                    fileBytes = await _watermarkService.ApplyWatermarkAsync(fileBytes, watermarkSettings);
                }

                await LogDownloadAsync(galleryId, photoId, userId, null);

                return new GalleryPhotoDownloadResult
                {
                    Status = GalleryDownloadStatus.Success,
                    FileBytes = fileBytes,
                    FileName = fileName,
                    ContentType = "image/jpeg"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing photo download");
                return new GalleryPhotoDownloadResult { Status = GalleryDownloadStatus.Error };
            }
        }

        public async Task<GalleryBulkDownloadResult> GetBulkDownloadAsync(int galleryId, List<int> photoIds, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new GalleryBulkDownloadResult { Status = GalleryDownloadStatus.Unauthorized };
                }

                if (photoIds == null || photoIds.Count == 0 || photoIds.Count > 500)
                {
                    return new GalleryBulkDownloadResult { Status = GalleryDownloadStatus.InvalidRequest };
                }

                var hasAccess = await ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                {
                    return new GalleryBulkDownloadResult { Status = GalleryDownloadStatus.Unauthorized };
                }

                if (!await CanUserDownloadAsync(galleryId, userId))
                {
                    return new GalleryBulkDownloadResult { Status = GalleryDownloadStatus.Forbidden };
                }

                var photos = await _context.Photos
                    .Include(p => p.Album)
                        .ThenInclude(a => a.Galleries)
                    .Where(p => photoIds.Contains(p.Id) && p.Album.Galleries.Any(g => g.Id == galleryId))
                    .AsNoTracking()
                    .ToListAsync();

                if (!photos.Any())
                {
                    return new GalleryBulkDownloadResult { Status = GalleryDownloadStatus.NotFound };
                }

                var gallery = await _context.Galleries.FindAsync(galleryId);
                var zipFileName = $"{gallery?.Name ?? "Photos"}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";

                var applyWatermark = gallery != null && gallery.WatermarkEnabled;
                WatermarkSettings? watermarkSettings = null;
                if (applyWatermark && gallery != null)
                {
                    watermarkSettings = CreateWatermarkSettings(gallery);
                }

                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var photoNumber = 1;

                    foreach (var photo in photos)
                    {
                        if (string.IsNullOrEmpty(photo.FullImagePath))
                            continue;

                        var filePath = FileSecurityHelper.GetSafeWwwrootPath(photo.FullImagePath, _logger);
                        if (filePath == null)
                        {
                            _logger.LogWarning($"Path traversal attempt detected during bulk download for photo: {photo.Id}");
                            continue;
                        }

                        if (!System.IO.File.Exists(filePath))
                        {
                            _logger.LogWarning($"Photo file not found during bulk download: {filePath}");
                            continue;
                        }

                        var extension = applyWatermark ? ".jpg" : Path.GetExtension(filePath);

                        var safeFileName = string.IsNullOrEmpty(photo.Title)
                            ? $"{photoNumber:D3}_photo_{photo.Id}{extension}"
                            : $"{photoNumber:D3}_{FileSecurityHelper.SanitizeFileName(photo.Title)}{extension}";

                        var zipEntry = archive.CreateEntry(safeFileName, CompressionLevel.Optimal);
                        using var zipEntryStream = zipEntry.Open();

                        if (applyWatermark && watermarkSettings != null)
                        {
                            var watermarkedBytes = await _watermarkService.ApplyWatermarkFromFileAsync(filePath, watermarkSettings);
                            await zipEntryStream.WriteAsync(watermarkedBytes);
                        }
                        else
                        {
                            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                            await fileStream.CopyToAsync(zipEntryStream);
                        }

                        photoNumber++;
                    }
                }

                memoryStream.Position = 0;

                return new GalleryBulkDownloadResult
                {
                    Status = GalleryDownloadStatus.Success,
                    FileBytes = memoryStream.ToArray(),
                    FileName = zipFileName,
                    PhotoCount = photos.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing bulk download");
                return new GalleryBulkDownloadResult { Status = GalleryDownloadStatus.Error };
            }
        }

        #endregion

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

        #region Private Helpers

        private static int ClampPageSize(int pageSize)
        {
            return Math.Min(Math.Max(pageSize, 12), 100);
        }

        private async Task<HashSet<string>> GetUserRolesAsync(string userId)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .Where(name => name != null)
                .Select(name => name!)
                .ToListAsync();

            return roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsStaffRole(IReadOnlyCollection<string> roles)
        {
            return roles.Contains("Admin") || roles.Contains("Photographer");
        }

        private async Task<bool> IsUserStaffAsync(string userId)
        {
            var roles = await GetUserRolesAsync(userId);
            return IsStaffRole(roles);
        }

        private async Task<bool> CanUserDownloadAsync(int galleryId, string userId)
        {
            if (await IsUserStaffAsync(userId))
                return true;

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (clientProfile == null)
                return false;

            var access = await _context.GalleryAccesses
                .FirstOrDefaultAsync(ga => ga.GalleryId == galleryId && ga.ClientProfileId == clientProfile.Id);

            return access != null && access.CanDownload;
        }

        private async Task<Gallery?> GetActiveGalleryAsync(int galleryId)
        {
            return await _context.Galleries
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == galleryId && g.IsActive && g.ExpiryDate > DateTime.UtcNow);
        }

        private async Task<Gallery?> GetPublicGalleryByTokenAsync(string token)
        {
            return await _context.Galleries
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.PublicAccessToken == token &&
                    g.AllowPublicAccess &&
                    g.IsActive &&
                    g.ExpiryDate > DateTime.UtcNow);
        }

        private async Task<Gallery?> GetPublicGalleryBySlugAsync(string slug)
        {
            return await _context.Galleries
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.Slug == slug &&
                    g.AllowPublicAccess &&
                    g.IsActive &&
                    g.ExpiryDate > DateTime.UtcNow);
        }

        private async Task<string> CreateAnonymousSessionAsync(int galleryId)
        {
            var sessionToken = Guid.NewGuid().ToString();
            var session = new GallerySession
            {
                GalleryId = galleryId,
                SessionToken = sessionToken,
                CreatedDate = DateTime.UtcNow,
                LastAccessDate = DateTime.UtcNow,
                UserId = null
            };
            _context.GallerySessions.Add(session);
            await _context.SaveChangesAsync();
            return sessionToken;
        }

        private async Task<string> GetOrCreateUserSessionTokenAsync(int galleryId, string userId)
        {
            var session = await _context.GallerySessions
                .FirstOrDefaultAsync(s => s.GalleryId == galleryId && s.UserId == userId);

            if (session == null)
            {
                session = new GallerySession
                {
                    GalleryId = galleryId,
                    UserId = userId,
                    SessionToken = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.UtcNow,
                    LastAccessDate = DateTime.UtcNow
                };
                _context.GallerySessions.Add(session);
            }
            else
            {
                session.LastAccessDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return session.SessionToken;
        }

        private async Task<GalleryPhotosPageResult?> GetGalleryPhotosPageInternalAsync(int galleryId, int page, int pageSize)
        {
            var gallery = await GetActiveGalleryAsync(galleryId);
            if (gallery == null)
                return null;

            var totalPhotos = await _context.Photos
                .Where(p => p.Album.Galleries.Any(g => g.Id == galleryId))
                .CountAsync();

            pageSize = ClampPageSize(pageSize);
            var photos = await _context.Photos
                .Where(p => p.Album.Galleries.Any(g => g.Id == galleryId))
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var paginatedPhotos = PaginatedList<Photo>.Create(photos, page, pageSize, totalPhotos);

            return new GalleryPhotosPageResult
            {
                Photos = paginatedPhotos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = paginatedPhotos.TotalPages,
                TotalCount = paginatedPhotos.TotalCount,
                HasNextPage = paginatedPhotos.HasNextPage,
                HasPreviousPage = paginatedPhotos.HasPreviousPage
            };
        }

        private static GalleryViewPageResult BuildGalleryViewPageResult(
            Gallery gallery,
            GalleryPhotosPageResult photoPage,
            string sessionToken,
            bool isPublicAccess)
        {
            return new GalleryViewPageResult
            {
                Gallery = gallery,
                Photos = photoPage.Photos,
                SessionToken = sessionToken,
                TotalPhotos = photoPage.TotalCount,
                PageSize = photoPage.PageSize,
                CurrentPage = photoPage.CurrentPage,
                TotalPages = photoPage.TotalPages,
                HasMorePhotos = photoPage.HasNextPage,
                DaysUntilExpiry = (gallery.ExpiryDate - DateTime.UtcNow).Days,
                IsPublicAccess = isPublicAccess
            };
        }

        private static WatermarkSettings CreateWatermarkSettings(Gallery gallery)
        {
            return new WatermarkSettings
            {
                Text = gallery.WatermarkText ?? "PROOF",
                ImagePath = gallery.WatermarkImagePath,
                Opacity = gallery.WatermarkOpacity,
                Position = gallery.WatermarkPosition,
                Tiled = gallery.WatermarkTiled,
                FontSizePercent = 5f,
                TileRotation = -30f,
                OutputQuality = 90
            };
        }

        #endregion
    }
}
