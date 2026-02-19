using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    [Authorize(Roles = "Client,Admin,Photographer")]
    public class GalleriesController : Controller
    {
        private readonly IGalleryService _galleryService;
        private readonly ILogger<GalleriesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public GalleriesController(
            IGalleryService galleryService,
            ILogger<GalleriesController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _galleryService = galleryService;
            _logger = logger;
            _userManager = userManager;
        }

        // Legacy route aliases for old singular Gallery URLs.
        [HttpGet("Gallery")]
        [HttpGet("Gallery/Index")]
        public IActionResult LegacyGalleryIndex()
        {
            return RedirectToAction(nameof(MyGalleries));
        }

        [HttpGet("Gallery/ViewGallery/{id:int}")]
        public Task<IActionResult> LegacyViewGallery(int id, int page = 1, int pageSize = 48)
        {
            return ViewGallery(id, page, pageSize);
        }

        [HttpGet("Gallery/Download")]
        public Task<IActionResult> LegacyDownload(int photoId, int galleryId)
        {
            return Download(photoId, galleryId);
        }

        [HttpPost("Gallery/DownloadBulk")]
        public Task<IActionResult> LegacyDownloadBulk(int galleryId, [FromBody] List<int> photoIds)
        {
            return DownloadBulk(galleryId, photoIds);
        }

        #region Admin Gallery Management

        // GET: Galleries
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var galleries = await _galleryService.GetAllGalleriesAsync();
                var stats = await _galleryService.GetGalleryStatsAsync();

                var viewModel = new GalleriesIndexViewModel
                {
                    Galleries = galleries.ToList(),
                    Stats = stats
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading galleries index");
                TempData["ErrorMessage"] = "An error occurred while loading galleries.";
                return View(new GalleriesIndexViewModel());
            }
        }

        // GET: Galleries/Details/5
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var gallery = await _galleryService.GetGalleryDetailsAsync(id);

                if (gallery == null)
                {
                    return NotFound();
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                gallery.AccessUrl = await _galleryService.GetGalleryAccessUrlAsync(id, baseUrl);

                return PartialView("_GalleryDetailsModal", gallery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading gallery details for ID: {GalleryId}", id);
                return StatusCode(500, "An error occurred while loading gallery details.");
            }
        }

        // GET: Galleries/Create
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new CreateGalleryViewModel
                {
                    AvailableAlbums = await _galleryService.GetAvailableAlbumsAsync(),
                    AvailableClients = await _galleryService.GetAvailableClientsAsync()
                };

                return PartialView("_CreateGalleryModal", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create gallery modal");
                return StatusCode(500, "An error occurred while loading the form.");
            }
        }

        // POST: Galleries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Create(CreateGalleryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableAlbums = await _galleryService.GetAvailableAlbumsAsync();
                    model.AvailableClients = await _galleryService.GetAvailableClientsAsync();
                    return Json(new
                    {
                        success = false,
                        message = "Please correct the errors in the form.",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var gallery = await _galleryService.CreateGalleryAsync(model);

                _logger.LogInformation("Gallery created: {GalleryName} (ID: {GalleryId})", gallery.Name, gallery.Id);

                return Json(new
                {
                    success = true,
                    message = $"Gallery '{gallery.Name}' created successfully!",
                    galleryId = gallery.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating gallery");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while creating the gallery. Please try again."
                });
            }
        }

        // GET: Galleries/Edit/5
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var gallery = await _galleryService.GetGalleryByIdAsync(id);

                if (gallery == null)
                {
                    return NotFound();
                }

                var model = new EditGalleryViewModel
                {
                    Id = gallery.Id,
                    Name = gallery.Name,
                    Description = gallery.Description,
                    ExpiryDate = gallery.ExpiryDate,
                    BrandColor = gallery.BrandColor,
                    IsActive = gallery.IsActive,
                    CreatedDate = gallery.CreatedDate,
                    WatermarkEnabled = gallery.WatermarkEnabled,
                    WatermarkText = gallery.WatermarkText,
                    WatermarkImagePath = gallery.WatermarkImagePath,
                    WatermarkOpacity = gallery.WatermarkOpacity,
                    WatermarkPosition = gallery.WatermarkPosition,
                    WatermarkTiled = gallery.WatermarkTiled,
                    SelectedAlbumIds = gallery.Albums.Select(a => a.Id).ToList(),
                    AvailableAlbums = await _galleryService.GetAvailableAlbumsAsync(id)
                };

                return PartialView("_EditGalleryModal", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit gallery modal for ID: {GalleryId}", id);
                return StatusCode(500, "An error occurred while loading the form.");
            }
        }

        // POST: Galleries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Edit(int id, EditGalleryViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    model.AvailableAlbums = await _galleryService.GetAvailableAlbumsAsync(id);
                    return Json(new
                    {
                        success = false,
                        message = "Please correct the errors in the form.",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var gallery = await _galleryService.UpdateGalleryAsync(model);

                _logger.LogInformation("Gallery updated: {GalleryName} (ID: {GalleryId})", gallery.Name, gallery.Id);

                return Json(new
                {
                    success = true,
                    message = $"Gallery '{gallery.Name}' updated successfully!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating gallery ID: {GalleryId}", id);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the gallery. Please try again."
                });
            }
        }

        // POST: Galleries/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _galleryService.DeleteGalleryAsync(id);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Gallery not found."
                    });
                }

                _logger.LogInformation("Gallery deleted: ID {GalleryId}", id);

                return Json(new
                {
                    success = true,
                    message = "Gallery deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting gallery ID: {GalleryId}", id);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while deleting the gallery. It may have active sessions."
                });
            }
        }

        // POST: Galleries/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            try
            {
                var result = await _galleryService.ToggleGalleryStatusAsync(id, isActive);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Gallery not found."
                    });
                }

                return Json(new
                {
                    success = true,
                    message = $"Gallery {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling gallery status for ID: {GalleryId}", id);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the gallery status."
                });
            }
        }

        // GET: Galleries/Sessions/5
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Sessions(int id)
        {
            try
            {
                var sessions = await _galleryService.GetGallerySessionsAsync(id);
                var gallery = await _galleryService.GetGalleryByIdAsync(id);

                if (gallery == null)
                {
                    return NotFound();
                }

                ViewBag.GalleryName = gallery.Name;
                ViewBag.GalleryId = id;

                return PartialView("_GallerySessionsModal", sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sessions for gallery ID: {GalleryId}", id);
                return StatusCode(500, "An error occurred while loading sessions.");
            }
        }

        // POST: Galleries/EndSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            try
            {
                var result = await _galleryService.EndSessionAsync(sessionId);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session not found."
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Session ended successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session ID: {SessionId}", sessionId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while ending the session."
                });
            }
        }

        // GET: Galleries/ManageAccess/5
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> ManageAccess(int id)
        {
            try
            {
                var gallery = await _galleryService.GetGalleryByIdAsync(id);
                if (gallery == null)
                {
                    return NotFound();
                }

                var accesses = await _galleryService.GetGalleryAccessesAsync(id);

                ViewBag.GalleryId = id;
                ViewBag.GalleryName = gallery.Name;

                return PartialView("_ManageAccessModal", accesses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading access management for gallery ID: {GalleryId}", id);
                return StatusCode(500, "An error occurred.");
            }
        }

        // POST: Galleries/GrantAccess
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> GrantAccess(int galleryId, int clientProfileId, DateTime? expiryDate = null)
        {
            try
            {
                await _galleryService.GrantAccessAsync(galleryId, clientProfileId, expiryDate);
                return Json(new { success = true, message = "Access granted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting access to gallery {GalleryId} for client {ClientProfileId}", galleryId, clientProfileId);
                return Json(new { success = false, message = "An error occurred while granting access." });
            }
        }

        // POST: Galleries/RevokeAccess
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> RevokeAccess(int galleryId, int clientProfileId)
        {
            try
            {
                var result = await _galleryService.RevokeAccessAsync(galleryId, clientProfileId);
                if (!result)
                {
                    return Json(new { success = false, message = "Access not found." });
                }
                return Json(new { success = true, message = "Access revoked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking access to gallery {GalleryId} from client {ClientProfileId}", galleryId, clientProfileId);
                return Json(new { success = false, message = "An error occurred while revoking access." });
            }
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> GetAccessUrl(int id)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var url = await _galleryService.GetGalleryAccessUrlAsync(id, baseUrl);

                var gallery = await _galleryService.GetGalleryByIdAsync(id);

                string accessType;
                string message;

                if (gallery != null)
                {
                    if (!string.IsNullOrEmpty(gallery.Slug))
                    {
                        accessType = "slug";
                        message = "Public SEO-friendly URL copied! Anyone with this link can view the gallery.";
                    }
                    else if (gallery.AllowPublicAccess && !string.IsNullOrEmpty(gallery.PublicAccessToken))
                    {
                        accessType = "public";
                        message = "Public access URL copied! Anyone with this link can view the gallery.";
                    }
                    else
                    {
                        accessType = "authenticated";
                        message = "Authenticated access URL copied! Only clients with granted access can view this gallery.";
                    }
                }
                else
                {
                    accessType = "authenticated";
                    message = "Gallery URL copied!";
                }

                return Json(new
                {
                    success = true,
                    url,
                    accessType,
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access URL for gallery ID: {GalleryId}", id);
                return Json(new
                {
                    success = false,
                    message = "An error occurred."
                });
            }
        }

        #endregion

        #region Client Gallery Views

        [HttpGet]
        public async Task<IActionResult> MyGalleries()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _galleryService.GetAccessibleGalleriesForUserAsync(userId);

            if (!result.HasProfile)
            {
                _logger.LogWarning("No client profile found for user: {UserId}", userId);
                return View("NoAccess");
            }

            return View("MyGalleries", result.Galleries);
        }

        [HttpGet]
        public async Task<IActionResult> ViewGallery(int id, int page = 1, int pageSize = 48)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _galleryService.GetGalleryViewPageForUserAsync(id, userId, page, pageSize);
                if (result == null)
                {
                    _logger.LogWarning("User {UserId} attempted to access gallery {GalleryId} without permission", userId, id);
                    return RedirectToAction(nameof(MyGalleries));
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

                return View("ViewGallery", result.Photos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing gallery {GalleryId}", id);
                TempData["Error"] = "An error occurred while loading the gallery. Please try again.";
                return RedirectToAction(nameof(MyGalleries));
            }
        }

        [HttpGet]
        [Route("api/gallery/{galleryId}/photos")]
        public async Task<IActionResult> GetPhotos(int galleryId, int page = 1, int pageSize = 48)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var hasAccess = await _galleryService.ValidateUserAccessAsync(galleryId, userId);
                if (!hasAccess)
                {
                    return Unauthorized(new { success = false, message = "No access to gallery" });
                }

                var paginatedPhotos = await _galleryService.GetGalleryPhotosPageAsync(galleryId, page, pageSize);
                if (paginatedPhotos == null)
                {
                    return NotFound(new { success = false, message = "Gallery not found or expired" });
                }

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
                _logger.LogError(ex, "Error loading photos for gallery {GalleryId}", galleryId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(int photoId, int galleryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var download = await _galleryService.GetPhotoDownloadAsync(galleryId, photoId, userId);
                switch (download.Status)
                {
                    case GalleryDownloadStatus.Unauthorized:
                        _logger.LogWarning("Download attempt without permission: user {UserId}, gallery {GalleryId}", userId, galleryId);
                        return Unauthorized();
                    case GalleryDownloadStatus.Forbidden:
                        _logger.LogWarning("Download not permitted for user {UserId} on gallery {GalleryId}", userId, galleryId);
                        return Forbid();
                    case GalleryDownloadStatus.NotFound:
                        _logger.LogWarning("Download attempt for non-existent photo: {PhotoId}", photoId);
                        return NotFound();
                    case GalleryDownloadStatus.Error:
                        return StatusCode(500);
                    default:
                        _logger.LogInformation("Photo downloaded: {PhotoId} by user: {UserId}", photoId, userId);
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

        [HttpPost]
        public async Task<IActionResult> DownloadBulk(int galleryId, [FromBody] List<int> photoIds)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var download = await _galleryService.GetBulkDownloadAsync(galleryId, photoIds, userId);
                switch (download.Status)
                {
                    case GalleryDownloadStatus.InvalidRequest:
                        return BadRequest("Invalid number of photos. Must be between 1 and 500.");
                    case GalleryDownloadStatus.Unauthorized:
                        _logger.LogWarning("Bulk download attempt without permission: user {UserId}, gallery {GalleryId}", userId, galleryId);
                        return Unauthorized();
                    case GalleryDownloadStatus.Forbidden:
                        _logger.LogWarning("Bulk download not permitted for user {UserId} on gallery {GalleryId}", userId, galleryId);
                        return Forbid();
                    case GalleryDownloadStatus.NotFound:
                        return NotFound("No valid photos found for download.");
                    case GalleryDownloadStatus.Error:
                        return StatusCode(500, "An error occurred while creating the download.");
                    default:
                        _logger.LogInformation("Bulk download: {PhotoCount} photos from gallery {GalleryId} by user {UserId}", download.PhotoCount, galleryId, userId);
                        return File(download.FileBytes, "application/zip", download.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk download");
                return StatusCode(500, "An error occurred while creating the download.");
            }
        }

        [HttpGet]
        [Route("api/gallery/session/{galleryId}")]
        public async Task<IActionResult> GetSessionInfo(int galleryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var sessionInfo = await _galleryService.GetGallerySessionInfoAsync(galleryId, userId);
                if (sessionInfo == null)
                {
                    return Unauthorized(new { success = false, message = "Gallery expired" });
                }

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

        [HttpPost]
        [Route("api/gallery/session/end/{galleryId}")]
        public async Task<IActionResult> EndGallerySession(int galleryId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { success = false, message = "Not authenticated" });
                }

                var ended = await _galleryService.EndGallerySessionAsync(galleryId, userId);
                if (!ended)
                {
                    return NotFound(new { success = false, message = "Session not found" });
                }

                return Ok(new { success = true, message = "Session ended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [AllowAnonymous]
        [HttpGet("gallery/access")]
        [HttpGet("Gallery/AccessGallery")]
        public async Task<IActionResult> AccessGallery(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                var galleryId = await _galleryService.GetGalleryIdByTokenAsync(token);
                if (galleryId.HasValue)
                {
                    return RedirectToAction(nameof(ViewPublicGallery), new { token });
                }

                TempData["Error"] = "That access code is invalid or may have expired. Please check and try again.";
            }

            return View("AccessGallery");
        }

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

                            _logger.LogInformation("Staff user {UserId} accessed gallery {GalleryId} via token", userId, staffResult.Gallery.Id);

                            return View("ViewGallery", staffResult.Photos.ToList());
                        }
                    }
                }

                var result = await _galleryService.GetPublicGalleryViewPageByTokenAsync(token, page, pageSize);
                if (result == null)
                {
                    _logger.LogWarning("Gallery not found or access denied for token: {Token}", token);
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

                _logger.LogInformation("Public gallery {GalleryId} accessed with token", result.Gallery.Id);

                return View("ViewGallery", result.Photos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing public gallery");
                return View("NoAccess");
            }
        }

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
                    _logger.LogWarning("Gallery not found or access denied for slug: {Slug}", slug);
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

                _logger.LogInformation("Public gallery {GalleryId} accessed via slug: {Slug}", result.Gallery.Id, slug);

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
