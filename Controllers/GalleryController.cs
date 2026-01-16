// Controllers/GalleryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.Controllers
{
    [Authorize(Roles = "Client,Admin")]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GalleryController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGalleryService _galleryService;
        private readonly IWatermarkService _watermarkService;

        public GalleryController(
            ApplicationDbContext context,
            ILogger<GalleryController> logger,
            UserManager<ApplicationUser> userManager,
            IGalleryService galleryService,
            IWatermarkService watermarkService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _galleryService = galleryService;
            _watermarkService = watermarkService;
        }

        /// <summary>
        /// Display list of accessible galleries for the logged-in client or admin
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // Check if user is admin
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                // Admins can see all active galleries
                var allGalleries = await _context.Galleries
                    .Include(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                    .Where(g => g.IsActive && g.ExpiryDate > DateTime.UtcNow)
                    .OrderByDescending(g => g.CreatedDate)
                    .ToListAsync();

                var adminViewModel = allGalleries.Select(gallery => new MyPhotoBiz.ViewModels.ClientGalleryViewModel
                {
                    GalleryId = gallery.Id,
                    Name = gallery.Name,
                    Description = gallery.Description,
                    BrandColor = gallery.BrandColor,
                    PhotoCount = gallery.Albums.SelectMany(a => a.Photos).Count(),
                    ExpiryDate = gallery.ExpiryDate,
                    GrantedDate = gallery.CreatedDate, // Use creation date for admins
                    CanDownload = true, // Admins have full permissions
                    CanProof = true,
                    CanOrder = true,
                    ThumbnailUrl = gallery.Albums
                        .SelectMany(a => a.Photos)
                        .OrderBy(p => p.DisplayOrder)
                        .FirstOrDefault()?.ThumbnailPath
                }).ToList();

                return View(adminViewModel);
            }

            // For regular clients, check ClientProfile
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userId);

            if (clientProfile == null)
            {
                _logger.LogWarning($"No client profile found for user: {userId}");
                return View("NoAccess");
            }

            // Get galleries the client has access to
            var accessibleGalleries = await _context.GalleryAccesses
                .Include(ga => ga.Gallery)
                    .ThenInclude(g => g.Albums)
                        .ThenInclude(a => a.Photos)
                .Where(ga => ga.ClientProfileId == clientProfile.Id &&
                            ga.IsActive &&
                            (!ga.ExpiryDate.HasValue || ga.ExpiryDate > DateTime.UtcNow) &&
                            ga.Gallery.IsActive &&
                            ga.Gallery.ExpiryDate > DateTime.UtcNow)
                .Select(ga => new
                {
                    Gallery = ga.Gallery,
                    Access = ga
                })
                .ToListAsync();

            var viewModel = accessibleGalleries.Select(item => new MyPhotoBiz.ViewModels.ClientGalleryViewModel
            {
                GalleryId = item.Gallery.Id,
                Name = item.Gallery.Name,
                Description = item.Gallery.Description,
                BrandColor = item.Gallery.BrandColor,
                PhotoCount = item.Gallery.Albums.SelectMany(a => a.Photos).Count(),
                ExpiryDate = item.Gallery.ExpiryDate,
                GrantedDate = item.Access.GrantedDate,
                CanDownload = item.Access.CanDownload,
                CanProof = item.Access.CanProof,
                CanOrder = item.Access.CanOrder,
                ThumbnailUrl = item.Gallery.Albums
                    .SelectMany(a => a.Photos)
                    .OrderBy(p => p.DisplayOrder)
                    .FirstOrDefault()?.ThumbnailPath
            }).ToList();

            return View(viewModel);
        }

        private const int DefaultPageSize = 48;

        /// <summary>
        /// Display gallery with photos (paginated)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewGallery(int id, int page = 1, int pageSize = 48)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                // Validate user has access to this gallery
                var hasAccess = await _galleryService.ValidateUserAccessAsync(id, userId);
                if (!hasAccess)
                {
                    _logger.LogWarning($"User {userId} attempted to access gallery {id} without permission");
                    return RedirectToAction("Index");
                }

                // Get gallery metadata (lightweight query without photos)
                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gallery == null || !gallery.IsActive || gallery.ExpiryDate < DateTime.UtcNow)
                {
                    return RedirectToAction("Index");
                }

                // Create or update session for tracking (works for both clients and admins)
                var session = await _context.GallerySessions
                    .FirstOrDefaultAsync(s => s.GalleryId == id && s.UserId == userId);

                if (session == null)
                {
                    session = new GallerySession
                    {
                        GalleryId = id,
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

                ViewBag.SessionToken = session.SessionToken;

                // Performance optimization: Use SQL-level pagination instead of loading all photos
                // Get total count first (fast query)
                var totalPhotos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == id))
                    .CountAsync();

                // Get only the photos needed for this page
                pageSize = Math.Min(Math.Max(pageSize, 12), 100); // Limit between 12-100
                var photos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == id))
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var paginatedPhotos = PaginatedList<Photo>.Create(photos, page, pageSize, totalPhotos);

                ViewBag.GalleryName = gallery.Name;
                ViewBag.BrandColor = gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = gallery.Id;
                ViewBag.TotalPhotos = totalPhotos;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = paginatedPhotos.TotalPages;
                ViewBag.HasMorePhotos = paginatedPhotos.HasNextPage;
                ViewBag.PageSize = pageSize;

                return View(paginatedPhotos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing gallery {id}");
                TempData["Error"] = "An error occurred while loading the gallery. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// API endpoint for loading more photos (infinite scroll)
        /// </summary>
        [HttpGet]
        [Route("api/gallery/{galleryId}/photos")]
        public async Task<IActionResult> GetPhotos(int galleryId, int page = 1, int pageSize = 48)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "Not authenticated" });

                var hasAccess = await _galleryService.ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                    return Unauthorized(new { success = false, message = "No access to gallery" });

                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == galleryId);

                if (gallery == null || !gallery.IsActive || gallery.ExpiryDate < DateTime.UtcNow)
                    return NotFound(new { success = false, message = "Gallery not found or expired" });

                // Performance optimization: Use SQL-level pagination
                var totalPhotos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == galleryId))
                    .CountAsync();

                pageSize = Math.Min(Math.Max(pageSize, 12), 100);
                var photos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == galleryId))
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var paginatedPhotos = PaginatedList<Photo>.Create(photos, page, pageSize, totalPhotos);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        photos = paginatedPhotos.Select(p => new
                        {
                            p.Id,
                            p.Title,
                            p.ThumbnailPath,
                            p.FullImagePath,
                            p.DisplayOrder
                        }),
                        pagination = new
                        {
                            currentPage = page,
                            pageSize,
                            totalPages = paginatedPhotos.TotalPages,
                            totalCount = paginatedPhotos.TotalCount,
                            hasNextPage = paginatedPhotos.HasNextPage,
                            hasPreviousPage = paginatedPhotos.HasPreviousPage
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading photos for gallery {galleryId}");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Download full resolution photo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(int photoId, int galleryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Validate user has access to this gallery
                var hasAccess = await _galleryService.ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                {
                    _logger.LogWarning($"Download attempt without permission: user {userId}, gallery {galleryId}");
                    return Unauthorized();
                }

                // Get gallery access to check download permission
                var clientProfile = await _context.ClientProfiles
                    .FirstOrDefaultAsync(cp => cp.UserId == userId);

                if (clientProfile != null)
                {
                    var access = await _context.GalleryAccesses
                        .FirstOrDefaultAsync(ga => ga.GalleryId == galleryId && ga.ClientProfileId == clientProfile.Id);

                    if (access != null && !access.CanDownload)
                    {
                        _logger.LogWarning($"Download not permitted for user {userId} on gallery {galleryId}");
                        return Forbid();
                    }
                }

                // Verify photo belongs to an album in the gallery
                var photo = await _context.Photos
                    .Include(p => p.Album)
                        .ThenInclude(a => a.Galleries)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == photoId && p.Album.Galleries.Any(g => g.Id == galleryId));

                if (photo == null)
                {
                    _logger.LogWarning($"Download attempt for non-existent photo: {photoId}");
                    return NotFound();
                }

                // Validate file path
                if (string.IsNullOrEmpty(photo.FullImagePath))
                {
                    _logger.LogWarning($"Photo has no file path: {photoId}");
                    return NotFound();
                }

                // Security: Validate path doesn't escape wwwroot
                var filePath = FileSecurityHelper.GetSafeWwwrootPath(photo.FullImagePath, _logger);
                if (filePath == null)
                {
                    _logger.LogWarning($"Path traversal attempt detected for photo: {photoId}");
                    return Unauthorized();
                }

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Photo file not found: {filePath}");
                    return NotFound();
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = string.IsNullOrEmpty(photo.Title) ? $"photo_{photo.Id}.jpg" : $"{photo.Title}.jpg";

                // Apply watermark if enabled for the gallery
                var gallery = await _context.Galleries.FindAsync(galleryId);
                if (gallery != null && gallery.WatermarkEnabled)
                {
                    var watermarkSettings = CreateWatermarkSettings(gallery);
                    fileBytes = await _watermarkService.ApplyWatermarkAsync(fileBytes, watermarkSettings);
                    _logger.LogInformation($"Watermark applied to photo {photo.Id} for download");
                }

                _logger.LogInformation($"Photo downloaded: {photo.Id} by user: {userId}");

                return File(fileBytes, "image/jpeg", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when downloading photo");
                return StatusCode(403);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading photo");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Download multiple photos as a ZIP file
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DownloadBulk(int galleryId, [FromBody] List<int> photoIds)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Validate photo IDs
                if (photoIds == null || !photoIds.Any() || photoIds.Count > 500)
                {
                    return BadRequest("Invalid number of photos. Must be between 1 and 500.");
                }

                // Validate user has access to this gallery
                var hasAccess = await _galleryService.ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                {
                    _logger.LogWarning($"Bulk download attempt without permission: user {userId}, gallery {galleryId}");
                    return Unauthorized();
                }

                // Check download permission
                var clientProfile = await _context.ClientProfiles
                    .FirstOrDefaultAsync(cp => cp.UserId == userId);

                if (clientProfile != null)
                {
                    var access = await _context.GalleryAccesses
                        .FirstOrDefaultAsync(ga => ga.GalleryId == galleryId && ga.ClientProfileId == clientProfile.Id);

                    if (access != null && !access.CanDownload)
                    {
                        _logger.LogWarning($"Bulk download not permitted for user {userId} on gallery {galleryId}");
                        return Forbid();
                    }
                }

                // Get photos
                var photos = await _context.Photos
                    .Include(p => p.Album)
                        .ThenInclude(a => a.Galleries)
                    .Where(p => photoIds.Contains(p.Id) && p.Album.Galleries.Any(g => g.Id == galleryId))
                    .AsNoTracking()
                    .ToListAsync();

                if (!photos.Any())
                {
                    return NotFound("No valid photos found for download.");
                }

                // Get gallery name for ZIP filename
                var gallery = await _context.Galleries.FindAsync(galleryId);
                var zipFileName = $"{gallery?.Name ?? "Photos"}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";

                // Check if watermarking is needed
                var applyWatermark = gallery != null && gallery.WatermarkEnabled;
                WatermarkSettings? watermarkSettings = null;
                if (applyWatermark && gallery != null)
                {
                    watermarkSettings = CreateWatermarkSettings(gallery);
                }

                // Create ZIP in memory
                using var memoryStream = new System.IO.MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    var photoNumber = 1;

                    foreach (var photo in photos)
                    {
                        if (string.IsNullOrEmpty(photo.FullImagePath))
                            continue;

                        // Security: Validate path doesn't escape wwwroot
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

                        // Get file extension - always use .jpg if watermarking (re-encoded as JPEG)
                        var extension = applyWatermark ? ".jpg" : Path.GetExtension(filePath);

                        // Create a safe filename with number prefix to avoid duplicates
                        var safeFileName = string.IsNullOrEmpty(photo.Title)
                            ? $"{photoNumber:D3}_photo_{photo.Id}{extension}"
                            : $"{photoNumber:D3}_{FileSecurityHelper.SanitizeFileName(photo.Title)}{extension}";

                        // Add file to ZIP
                        var zipEntry = archive.CreateEntry(safeFileName, System.IO.Compression.CompressionLevel.Optimal);
                        using var zipEntryStream = zipEntry.Open();

                        if (applyWatermark && watermarkSettings != null)
                        {
                            // Apply watermark and write to ZIP
                            var watermarkedBytes = await _watermarkService.ApplyWatermarkFromFileAsync(filePath, watermarkSettings);
                            await zipEntryStream.WriteAsync(watermarkedBytes);
                        }
                        else
                        {
                            // Copy original file to ZIP
                            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                            await fileStream.CopyToAsync(zipEntryStream);
                        }

                        photoNumber++;
                    }
                }

                memoryStream.Position = 0;

                _logger.LogInformation($"Bulk download: {photos.Count} photos from gallery {galleryId} by user {userId}");

                return File(memoryStream.ToArray(), "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk download");
                return StatusCode(500, "An error occurred while creating the download.");
            }
        }


        /// <summary>
        /// Get gallery session info via API
        /// </summary>
        [HttpGet]
        [Route("api/gallery/session/{galleryId}")]
        public async Task<IActionResult> GetSessionInfo(int galleryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "Not authenticated" });

                var hasAccess = await _galleryService.ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                    return Unauthorized(new { success = false, message = "No access to gallery" });

                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == galleryId);

                if (gallery == null || !gallery.IsActive || gallery.ExpiryDate < DateTime.UtcNow)
                    return Unauthorized(new { success = false, message = "Gallery expired" });

                var session = await _context.GallerySessions
                    .FirstOrDefaultAsync(s => s.GalleryId == galleryId && s.UserId == userId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        GalleryId = gallery.Id,
                        gallery.Name,
                        gallery.Description,
                        gallery.BrandColor,
                        gallery.LogoPath,
                        gallery.ExpiryDate,
                        CreatedDate = session?.CreatedDate,
                        LastAccessDate = session?.LastAccessDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session info");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// End gallery session
        /// </summary>
        [HttpPost]
        [Route("api/gallery/session/end/{galleryId}")]
        public async Task<IActionResult> EndSession(int galleryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { success = false, message = "Not authenticated" });

                var session = await _context.GallerySessions
                    .FirstOrDefaultAsync(s => s.GalleryId == galleryId && s.UserId == userId);

                if (session == null)
                    return NotFound(new { success = false, message = "Session not found" });

                _context.GallerySessions.Remove(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery session ended for user {userId} on gallery {galleryId}");

                return Ok(new { success = true, message = "Session ended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        #region Public Gallery Access (Token-based, no authentication required)

        /// <summary>
        /// View gallery using public access token (no authentication required)
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("gallery/view/{token}")]
        public async Task<IActionResult> ViewPublicGallery(string token, int page = 1, int pageSize = 48)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Public gallery access attempted with empty token");
                    return View("NoAccess");
                }

                // Get gallery metadata only (lightweight query)
                var gallery = await _galleryService.GetGalleryByPublicTokenAsync(token);

                if (gallery == null)
                {
                    _logger.LogWarning($"Gallery not found or access denied for token: {token}");
                    return View("NoAccess");
                }

                // Create anonymous session for tracking
                var sessionToken = Guid.NewGuid().ToString();
                var session = new GallerySession
                {
                    GalleryId = gallery.Id,
                    SessionToken = sessionToken,
                    CreatedDate = DateTime.UtcNow,
                    LastAccessDate = DateTime.UtcNow,
                    UserId = null // Anonymous access
                };
                _context.GallerySessions.Add(session);
                await _context.SaveChangesAsync();

                // Performance optimization: Use SQL-level pagination
                var totalPhotos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == gallery.Id))
                    .CountAsync();

                pageSize = Math.Min(Math.Max(pageSize, 12), 100);
                var photos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == gallery.Id))
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var paginatedPhotos = PaginatedList<Photo>.Create(photos, page, pageSize, totalPhotos);

                ViewBag.GalleryName = gallery.Name;
                ViewBag.BrandColor = gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = gallery.Id;
                ViewBag.SessionToken = sessionToken;
                ViewBag.TotalPhotos = totalPhotos;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = paginatedPhotos.TotalPages;
                ViewBag.HasMorePhotos = paginatedPhotos.HasNextPage;
                ViewBag.PageSize = pageSize;
                ViewBag.IsPublicAccess = true;

                _logger.LogInformation($"Public gallery {gallery.Id} accessed with token");

                return View("ViewGallery", paginatedPhotos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing public gallery");
                return View("NoAccess");
            }
        }

        /// <summary>
        /// View gallery using SEO-friendly slug (no authentication required)
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("gallery/{slug}")]
        public async Task<IActionResult> ViewPublicGalleryBySlug(string slug, int page = 1, int pageSize = 48)
        {
            try
            {
                if (string.IsNullOrEmpty(slug))
                {
                    return View("NoAccess");
                }

                // Get gallery metadata only (lightweight query)
                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Slug == slug && g.AllowPublicAccess && g.IsActive && g.ExpiryDate > DateTime.UtcNow);

                if (gallery == null)
                {
                    _logger.LogWarning($"Gallery not found or access denied for slug: {slug}");
                    return View("NoAccess");
                }

                // Create anonymous session for tracking
                var sessionToken = Guid.NewGuid().ToString();
                var session = new GallerySession
                {
                    GalleryId = gallery.Id,
                    SessionToken = sessionToken,
                    CreatedDate = DateTime.UtcNow,
                    LastAccessDate = DateTime.UtcNow,
                    UserId = null
                };
                _context.GallerySessions.Add(session);
                await _context.SaveChangesAsync();

                // Performance optimization: Use SQL-level pagination
                var totalPhotos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == gallery.Id))
                    .CountAsync();

                pageSize = Math.Min(Math.Max(pageSize, 12), 100);
                var photos = await _context.Photos
                    .Where(p => p.Album.Galleries.Any(g => g.Id == gallery.Id))
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var paginatedPhotos = PaginatedList<Photo>.Create(photos, page, pageSize, totalPhotos);

                ViewBag.GalleryName = gallery.Name;
                ViewBag.BrandColor = gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = gallery.Id;
                ViewBag.SessionToken = sessionToken;
                ViewBag.TotalPhotos = totalPhotos;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = paginatedPhotos.TotalPages;
                ViewBag.HasMorePhotos = paginatedPhotos.HasNextPage;
                ViewBag.PageSize = pageSize;
                ViewBag.IsPublicAccess = true;

                _logger.LogInformation($"Public gallery {gallery.Id} accessed via slug: {slug}");

                return View("ViewGallery", paginatedPhotos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing public gallery by slug");
                return View("NoAccess");
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Creates watermark settings from gallery configuration
        /// </summary>
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
