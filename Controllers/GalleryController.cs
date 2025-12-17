// Controllers/GalleryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyPhotoBiz.Controllers
{
    [AllowAnonymous] // Gallery accessible to clients via session token
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GalleryController> _logger;

        public GalleryController(ApplicationDbContext context, ILogger<GalleryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Display gallery login page
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Authenticate and create gallery session
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccessGallery(string clientCode, string clientPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientCode) || string.IsNullOrWhiteSpace(clientPassword))
                {
                    ModelState.AddModelError("", "Gallery code and password are required");
                    return View("Index");
                }

                var gallery = await _context.Galleries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.ClientCode == clientCode && 
                                             g.ClientPassword == clientPassword &&
                                             g.IsActive &&
                                             g.ExpiryDate > DateTime.UtcNow);

                if (gallery == null)
                {
                    _logger.LogWarning($"Failed gallery access attempt with code: {clientCode}");
                    ModelState.AddModelError("", "Invalid gallery code or password");
                    return View("Index");
                }

                var sessionToken = Guid.NewGuid().ToString();
                var session = new GallerySession
                {
                    GalleryId = gallery.Id,
                    SessionToken = sessionToken,
                    CreatedDate = DateTime.UtcNow,
                    LastAccessDate = DateTime.UtcNow,
                    Gallery = gallery
                };

                _context.GallerySessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery session created: {sessionToken} for gallery: {gallery.Name}");

                return RedirectToAction("ViewGallery", new { sessionToken });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error during gallery access");
                ModelState.AddModelError("", "An error occurred while accessing the gallery. Please try again.");
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing gallery");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("Index");
            }
        }

        /// <summary>
        /// Display gallery with photos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewGallery(string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionToken))
                    return RedirectToAction("Index");

                var session = await _context.GallerySessions
                    .Include(s => s.Gallery)
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                if (session == null)
                {
                    _logger.LogWarning($"Invalid session token attempted: {sessionToken}");
                    return RedirectToAction("Index");
                }

                // Validate gallery is still active and not expired
                if (!session.Gallery.IsActive || session.Gallery.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Gallery access attempt on inactive/expired gallery: {session.Gallery.Id}");
                    return RedirectToAction("Index");
                }

                // Update last access time
                session.LastAccessDate = DateTime.UtcNow;
                _context.GallerySessions.Update(session);
                await _context.SaveChangesAsync();

                var photos = await _context.Photos
                    .Where(p => p.GalleryId == session.GalleryId)
                    .AsNoTracking()
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();

                ViewBag.SessionToken = sessionToken;
                ViewBag.GalleryName = session.Gallery.Name;
                ViewBag.BrandColor = session.Gallery.BrandColor ?? "#2c3e50";
                ViewBag.GalleryId = session.GalleryId;

                return View(photos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing gallery");
                TempData["Error"] = "An error occurred while loading the gallery. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Download full resolution photo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(int photoId, string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionToken))
                    return Unauthorized();

                var session = await _context.GallerySessions
                    .Include(s => s.Gallery)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                if (session == null)
                {
                    _logger.LogWarning($"Download attempt with invalid session token");
                    return Unauthorized();
                }

                // Validate gallery is still active
                if (!session.Gallery.IsActive || session.Gallery.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Download attempt on inactive/expired gallery: {session.GalleryId}");
                    return Unauthorized();
                }

                var photo = await _context.Photos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == photoId && p.GalleryId == session.GalleryId);

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

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FullImagePath.TrimStart('/'));

                // Security: Validate path doesn't escape wwwroot
                var fullWwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var resolvedPath = Path.GetFullPath(filePath);
                if (!resolvedPath.StartsWith(fullWwwrootPath))
                {
                    _logger.LogWarning($"Path traversal attempt detected: {filePath}");
                    return Unauthorized();
                }

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Photo file not found: {filePath}");
                    return NotFound();
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = string.IsNullOrEmpty(photo.Title) ? $"photo_{photo.Id}.jpg" : $"{photo.Title}.jpg";
                
                _logger.LogInformation($"Photo downloaded: {photo.Id} by session: {sessionToken}");

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
        /// Get gallery session info via API
        /// </summary>
        [HttpGet]
        [Route("api/gallery/session/{sessionToken}")]
        public async Task<IActionResult> GetSessionInfo(string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionToken))
                    return BadRequest(new { success = false, message = "Session token required" });

                var session = await _context.GallerySessions
                    .Include(s => s.Gallery)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                if (session == null)
                    return Unauthorized(new { success = false, message = "Invalid session" });

                if (!session.Gallery.IsActive || session.Gallery.ExpiryDate < DateTime.UtcNow)
                    return Unauthorized(new { success = false, message = "Gallery expired" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        session.GalleryId,
                        session.Gallery.Name,
                        session.Gallery.Description,
                        session.Gallery.BrandColor,
                        session.Gallery.LogoPath,
                        ExpiryDate = session.Gallery.ExpiryDate,
                        CreatedDate = session.CreatedDate,
                        LastAccessDate = session.LastAccessDate
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
        [Route("api/gallery/session/end/{sessionToken}")]
        public async Task<IActionResult> EndSession(string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionToken))
                    return BadRequest(new { success = false, message = "Session token required" });

                var session = await _context.GallerySessions
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                if (session == null)
                    return NotFound(new { success = false, message = "Session not found" });

                _context.GallerySessions.Remove(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Gallery session ended: {sessionToken}");

                return Ok(new { success = true, message = "Session ended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}