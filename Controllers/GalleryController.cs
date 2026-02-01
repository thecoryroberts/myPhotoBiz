// Controllers/GalleryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    [Authorize(Roles = "Client,Admin,Photographer")]
    public class GalleryController : Controller
    {
        private readonly ILogger<GalleryController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGalleryService _galleryService;

        public GalleryController(
            ILogger<GalleryController> logger,
            UserManager<ApplicationUser> userManager,
            IGalleryService galleryService)
        {
            _logger = logger;
            _userManager = userManager;
            _galleryService = galleryService;
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

            var result = await _galleryService.GetAccessibleGalleriesForUserAsync(userId);

            if (!result.HasProfile)
            {
                _logger.LogWarning($"No client profile found for user: {userId}");
                return View("NoAccess");
            }

            return View(result.Galleries);
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

                var result = await _galleryService.GetGalleryViewPageForUserAsync(id, userId, page, pageSize);
                if (result == null)
                {
                    _logger.LogWarning($"User {userId} attempted to access gallery {id} without permission");
                    return RedirectToAction("Index");
                }

                ViewBag.SessionToken = result.SessionToken;
                ViewBag.GalleryName = result.Gallery.Name;
                ViewBag.BrandColor = result.Gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = result.Gallery.Id;
                ViewBag.TotalPhotos = result.TotalPhotos;
                ViewBag.CurrentPage = result.CurrentPage;
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.HasMorePhotos = result.HasMorePhotos;
                ViewBag.PageSize = result.PageSize;
                ViewBag.ExpiryDate = result.Gallery.ExpiryDate;
                ViewBag.DaysUntilExpiry = result.DaysUntilExpiry;

                return View(result.Photos.ToList());
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

                var paginatedPhotos = await _galleryService.GetGalleryPhotosPageAsync(galleryId, page, pageSize);
                if (paginatedPhotos == null)
                    return NotFound(new { success = false, message = "Gallery not found or expired" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        photos = paginatedPhotos.Photos.Select(p => new
                        {
                            p.Id,
                            p.Title,
                            p.ThumbnailPath,
                            p.FullImagePath,
                            p.DisplayOrder
                        }),
                        pagination = new
                        {
                            currentPage = paginatedPhotos.CurrentPage,
                            pageSize = paginatedPhotos.PageSize,
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

                var download = await _galleryService.GetPhotoDownloadAsync(galleryId, photoId, userId);
                switch (download.Status)
                {
                    case GalleryDownloadStatus.Unauthorized:
                        _logger.LogWarning($"Download attempt without permission: user {userId}, gallery {galleryId}");
                        return Unauthorized();
                    case GalleryDownloadStatus.Forbidden:
                        _logger.LogWarning($"Download not permitted for user {userId} on gallery {galleryId}");
                        return Forbid();
                    case GalleryDownloadStatus.NotFound:
                        _logger.LogWarning($"Download attempt for non-existent photo: {photoId}");
                        return NotFound();
                    case GalleryDownloadStatus.Error:
                        return StatusCode(500);
                    default:
                        _logger.LogInformation($"Photo downloaded: {photoId} by user: {userId}");
                        return File(download.FileBytes, download.ContentType, download.FileName);
                }
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

                var download = await _galleryService.GetBulkDownloadAsync(galleryId, photoIds, userId);
                switch (download.Status)
                {
                    case GalleryDownloadStatus.InvalidRequest:
                        return BadRequest("Invalid number of photos. Must be between 1 and 500.");
                    case GalleryDownloadStatus.Unauthorized:
                        _logger.LogWarning($"Bulk download attempt without permission: user {userId}, gallery {galleryId}");
                        return Unauthorized();
                    case GalleryDownloadStatus.Forbidden:
                        _logger.LogWarning($"Bulk download not permitted for user {userId} on gallery {galleryId}");
                        return Forbid();
                    case GalleryDownloadStatus.NotFound:
                        return NotFound("No valid photos found for download.");
                    case GalleryDownloadStatus.Error:
                        return StatusCode(500, "An error occurred while creating the download.");
                    default:
                        _logger.LogInformation($"Bulk download: {download.PhotoCount} photos from gallery {galleryId} by user {userId}");
                        return File(download.FileBytes, "application/zip", download.FileName);
                }
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

                var sessionInfo = await _galleryService.GetGallerySessionInfoAsync(galleryId, userId);
                if (sessionInfo == null)
                    return Unauthorized(new { success = false, message = "Gallery expired" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        GalleryId = sessionInfo.Gallery.Id,
                        sessionInfo.Gallery.Name,
                        sessionInfo.Gallery.Description,
                        sessionInfo.Gallery.BrandColor,
                        sessionInfo.Gallery.LogoPath,
                        sessionInfo.Gallery.ExpiryDate,
                        CreatedDate = sessionInfo.Session?.CreatedDate,
                        LastAccessDate = sessionInfo.Session?.LastAccessDate
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

                var ended = await _galleryService.EndGallerySessionAsync(galleryId, userId);
                if (!ended)
                    return NotFound(new { success = false, message = "Session not found" });

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

                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId) && (User.IsInRole("Admin") || User.IsInRole("Photographer") || User.IsInRole("SuperAdmin")))
                {
                    var galleryId = await _galleryService.GetGalleryIdByTokenAsync(token);
                    if (galleryId.HasValue)
                    {
                        var staffResult = await _galleryService.GetGalleryViewPageForUserAsync(galleryId.Value, userId, page, pageSize);
                        if (staffResult != null)
                        {
                            ViewBag.SessionToken = staffResult.SessionToken;
                            ViewBag.GalleryName = staffResult.Gallery.Name;
                            ViewBag.BrandColor = staffResult.Gallery.BrandColor ?? "#2c3e50";
                            ViewBag.GalleryId = staffResult.Gallery.Id;
                            ViewBag.TotalPhotos = staffResult.TotalPhotos;
                            ViewBag.CurrentPage = staffResult.CurrentPage;
                            ViewBag.TotalPages = staffResult.TotalPages;
                            ViewBag.HasMorePhotos = staffResult.HasMorePhotos;
                            ViewBag.PageSize = staffResult.PageSize;
                            ViewBag.ExpiryDate = staffResult.Gallery.ExpiryDate;
                            ViewBag.DaysUntilExpiry = staffResult.DaysUntilExpiry;

                            _logger.LogInformation($"Staff user {userId} accessed gallery {staffResult.Gallery.Id} via token");

                            return View("ViewGallery", staffResult.Photos.ToList());
                        }
                    }
                }

                var result = await _galleryService.GetPublicGalleryViewPageByTokenAsync(token, page, pageSize);
                if (result == null)
                {
                    _logger.LogWarning($"Gallery not found or access denied for token: {token}");
                    return View("NoAccess");
                }

                ViewBag.GalleryName = result.Gallery.Name;
                ViewBag.BrandColor = result.Gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = result.Gallery.Id;
                ViewBag.SessionToken = result.SessionToken;
                ViewBag.TotalPhotos = result.TotalPhotos;
                ViewBag.CurrentPage = result.CurrentPage;
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.HasMorePhotos = result.HasMorePhotos;
                ViewBag.PageSize = result.PageSize;
                ViewBag.IsPublicAccess = true;

                _logger.LogInformation($"Public gallery {result.Gallery.Id} accessed with token");

                return View("ViewGallery", result.Photos.ToList());
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

                var result = await _galleryService.GetPublicGalleryViewPageBySlugAsync(slug, page, pageSize);
                if (result == null)
                {
                    _logger.LogWarning($"Gallery not found or access denied for slug: {slug}");
                    return View("NoAccess");
                }

                ViewBag.GalleryName = result.Gallery.Name;
                ViewBag.BrandColor = result.Gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = result.Gallery.Id;
                ViewBag.SessionToken = result.SessionToken;
                ViewBag.TotalPhotos = result.TotalPhotos;
                ViewBag.CurrentPage = result.CurrentPage;
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.HasMorePhotos = result.HasMorePhotos;
                ViewBag.PageSize = result.PageSize;
                ViewBag.IsPublicAccess = true;

                _logger.LogInformation($"Public gallery {result.Gallery.Id} accessed via slug: {slug}");

                return View("ViewGallery", result.Photos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing public gallery by slug");
                return View("NoAccess");
            }
        }

        #endregion

    }
}
