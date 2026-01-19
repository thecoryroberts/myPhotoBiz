using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Extensions;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Controller for managing photo uploads, downloads, and gallery assignments.
    /// Supports photo metadata, favoriting, and client access permissions.
    /// </summary>
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly IPhotoService _photoService;
        private readonly IAlbumService _albumService;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private readonly IFileService _fileService;
        private readonly IPhotoAccessService _photoAccessService;
        private readonly ApplicationDbContext _context;

        public PhotosController(
            IPhotoService photoService,
            IAlbumService albumService,
            IWebHostEnvironment environment,
            IImageService imageService,
            IFileService fileService,
            IPhotoAccessService photoAccessService,
            ApplicationDbContext context)
        {
            _photoService = photoService;
            _albumService = albumService;
            _environment = environment;
            _imageService = imageService;
            _fileService = fileService;
            _photoAccessService = photoAccessService;
            _context = context;
        }

        /// <summary>
        /// Displays a gallery of all photos with filtering by album/client
        /// </summary>
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Index(int? albumId = null, int? clientId = null)
        {
            var query = _context.Photos
                .Include(p => p.Album)
                    .ThenInclude(a => a.PhotoShoot)
                        .ThenInclude(ps => ps.ClientProfile)
                            .ThenInclude(c => c.User)
                .AsQueryable();

            if (albumId.HasValue)
            {
                query = query.Where(p => p.AlbumId == albumId.Value);
                var album = await _albumService.GetAlbumByIdAsync(albumId.Value);
                ViewBag.FilterName = album?.Name ?? "Album";
            }
            else if (clientId.HasValue)
            {
                query = query.Where(p => p.Album.PhotoShoot.ClientProfileId == clientId.Value);
                var client = await _context.ClientProfiles
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == clientId.Value);
                ViewBag.FilterName = client?.User != null
                    ? $"{client.User.FirstName} {client.User.LastName}"
                    : "Client";
            }

            var photos = await query
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();

            // Get unique albums for filter buttons
            var albums = await _context.Albums
                .Include(a => a.PhotoShoot)
                    .ThenInclude(ps => ps.ClientProfile)
                        .ThenInclude(c => c.User)
                .Where(a => a.Photos.Any())
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Albums = albums;
            ViewBag.TotalPhotos = photos.Count;

            return View(photos);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Upload(int albumId)
        {
            var album = await _albumService.GetAlbumByIdAsync(albumId);
            if (album == null)
            {
                return NotFound();
            }

            ViewBag.AlbumId = albumId;
            ViewBag.AlbumName = album.Name;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Upload(int albumId, List<IFormFile> files)
        {
            var album = await _albumService.GetAlbumByIdAsync(albumId);
            if (album == null)
            {
                return NotFound();
            }

            if (files != null && files.Count > 0)
            {
                // Create the uploads directory path per-album
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "albums", albumId.ToString());
                Directory.CreateDirectory(uploadsPath);

                const long maxBytes = AppConstants.FileSizes.MaxPhotoUploadBytes;

                // Build naming components from photoshoot data
                var photoShoot = album.PhotoShoot;
                var clientName = photoShoot?.ClientProfile?.User != null
                    ? $"{photoShoot.ClientProfile.User.FirstName}_{photoShoot.ClientProfile.User.LastName}"
                    : "Client";
                var shootName = FileHelper.SanitizeFileName(photoShoot?.Title ?? "Photoshoot");
                var shootDate = photoShoot?.ScheduledDate.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

                // Counter for sequential numbering within this upload batch
                int photoCounter = 1;

                foreach (var file in files)
                {
                    if (file == null) continue;
                    if (file.Length == 0) continue;
                    if (file.Length > maxBytes)
                    {
                        // skip overly large files; optionally add a TempData warning
                        continue;
                    }
                    if (!FileHelper.IsImageFile(file))
                    {
                        // skip non-images
                        continue;
                    }

                    // Generate descriptive filename: ClientName_PhotoshootTitle_Date_001.jpg
                    var displayFileName = $"{clientName}_{shootName}_{shootDate}_{photoCounter:D3}{Path.GetExtension(file.FileName)}";

                    // generate base name and process via ImageService (creates fullsize + thumbnail)
                    var baseName = Guid.NewGuid().ToString();
                    try
                    {
                        var (filePath, thumbPath, publicUrl) = await _imageService.ProcessAndSaveAlbumImageAsync(file, albumId, baseName);

                        // Convert absolute paths to relative web paths
                        var relativeFilePath = $"/uploads/albums/{albumId}/{baseName}.jpg";
                        var relativeThumbnailPath = $"/uploads/albums/{albumId}/{baseName}_thumb.jpg";

                        var photo = new Photo
                        {
                            FileName = displayFileName, // Use descriptive filename
                            FilePath = relativeFilePath,
                            ThumbnailPath = relativeThumbnailPath,
                            FullImagePath = relativeFilePath, // Also set FullImagePath
                            FileSize = file.Length,
                            AlbumId = albumId,
                            ClientProfileId = album.ClientProfileId,
                            UploadDate = DateTime.Now,
                            UploadedDate = DateTime.Now,
                            DisplayOrder = 0,
                            IsSelected = false
                        };

                        await _photoService.CreatePhotoAsync(photo);

                        // Copy photo to client's folder in File Manager
                        var clientProfile = album.ClientProfile ?? album.PhotoShoot?.ClientProfile;
                        if (clientProfile != null)
                        {
                            try
                            {
                                // Create folder for client if they don't have one yet
                                if (clientProfile.FolderId == null)
                                {
                                    var clientFullName = clientProfile.User != null
                                        ? $"{clientProfile.User.FirstName} {clientProfile.User.LastName}"
                                        : $"Client_{clientProfile.Id}";
                                    var folder = await _fileService.CreateClientFolderAsync(clientFullName, User.Identity?.Name ?? "System");
                                    clientProfile.FolderId = folder.Id;
                                    // Save the FolderId to the database
                                    await _context.SaveChangesAsync();
                                }

                                await _fileService.CopyPhotoToClientFolderAsync(
                                    clientProfile.FolderId.Value,
                                    filePath, // absolute path from ImageService
                                    displayFileName, // Use descriptive filename
                                    User.Identity?.Name ?? "System"
                                );
                            }
                            catch (Exception copyEx)
                            {
                                // Log but don't fail upload if copy fails
                                System.Diagnostics.Debug.WriteLine($"Failed to copy photo to client folder: {copyEx.Message}");
                            }
                        }

                        photoCounter++;
                    }
                    catch (InvalidOperationException ex)
                    {
                        // collect warnings in a TempData string list stored as JSON
                        var warnings = new List<string>();
                        if (TempData.ContainsKey("UploadWarnings"))
                        {
                            var prev = TempData["UploadWarnings"] as string;
                            if (!string.IsNullOrEmpty(prev))
                            {
                                try { warnings = System.Text.Json.JsonSerializer.Deserialize<List<string>>(prev) ?? new List<string>(); } catch { warnings = new List<string>(); }
                            }
                        }
                        warnings.Add($"{file.FileName}: {ex.Message}");
                        TempData["UploadWarnings"] = System.Text.Json.JsonSerializer.Serialize(warnings);
                    }
                }

                TempData["SuccessMessage"] = $"{files.Count} photo(s) uploaded successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "No files were selected for upload.";
            }

            return RedirectToAction("Details", "Albums", new { id = albumId });
        }

        public async Task<IActionResult> View(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            // Use centralized access check (staff can access all, clients only their own)
            if (!this.IsStaffUser())
            {
                var accessResult = await _photoAccessService.CanAccessPhotoAsync(photo, User);
                if (!accessResult.IsAllowed)
                {
                    return Forbid();
                }
            }

            return await ServePhotoFileAsync(photo.FilePath);
        }

        /// <summary>
        /// Returns the full-size image file for lightbox display.
        /// AllowAnonymous for public gallery access.
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Image(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            // Use centralized access check
            var accessResult = await _photoAccessService.CanAccessPhotoAsync(photo, User);
            if (!accessResult.IsAllowed)
            {
                return Forbid();
            }

            return await ServePhotoFileAsync(photo.FilePath);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Thumbnail(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            // Use centralized access check
            var accessResult = await _photoAccessService.CanAccessPhotoAsync(photo, User);
            if (!accessResult.IsAllowed)
            {
                return Forbid();
            }

            // Return thumbnail, fall back to full image if thumbnail doesn't exist
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var absoluteThumbPath = !string.IsNullOrEmpty(photo.ThumbnailPath)
                ? FileHelper.GetAbsolutePath(photo.ThumbnailPath, webRootPath)
                : null;

            var thumbnailPath = (!string.IsNullOrEmpty(absoluteThumbPath) && System.IO.File.Exists(absoluteThumbPath))
                ? absoluteThumbPath
                : FileHelper.GetAbsolutePath(photo.FilePath, webRootPath);

            return await ServePhotoFileAsync(thumbnailPath, isAbsolutePath: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Delete(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            var albumId = photo.AlbumId;
            var deleted = await _photoService.DeletePhotoAsync(id);

            if (deleted)
            {
                TempData["SuccessMessage"] = "Photo deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete photo.";
            }

            return RedirectToAction("Details", "Albums", new { id = albumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> ToggleSelection(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            photo.IsSelected = !photo.IsSelected;
            await _photoService.UpdatePhotoAsync(photo);

            return RedirectToAction("Details", "Albums", new { id = photo.AlbumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> BulkDelete(int albumId, int[] photoIds)
        {
            if (photoIds == null || photoIds.Length == 0)
            {
                TempData["ErrorMessage"] = "No photos selected for deletion.";
                return RedirectToAction("Details", "Albums", new { id = albumId });
            }

            int deletedCount = 0;
            foreach (var id in photoIds)
            {
                try
                {
                    var ok = await _photoService.DeletePhotoAsync(id);
                    if (ok) deletedCount++;
                }
                catch
                {
                    // ignore individual delete errors but continue
                }
            }

            TempData["SuccessMessage"] = $"Deleted {deletedCount} photo(s).";
            return RedirectToAction("Details", "Albums", new { id = albumId });
        }

        #region Private Helpers

        /// <summary>
        /// Serves a photo file with proper MIME type
        /// </summary>
        private async Task<IActionResult> ServePhotoFileAsync(string? path, bool isAbsolutePath = false)
        {
            if (string.IsNullOrEmpty(path))
                return NotFound();

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var absolutePath = isAbsolutePath ? path : FileHelper.GetAbsolutePath(path, webRootPath);

            var memory = await FileHelper.ReadFileToMemoryAsync(absolutePath);
            if (memory == null)
                return NotFound();

            var mimeType = FileHelper.GetMimeType(absolutePath);
            return File(memory, mimeType);
        }

        #endregion
    }
}