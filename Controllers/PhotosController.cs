using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Data;
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
        private readonly IClientService _clientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;

        public PhotosController(IPhotoService photoService, IAlbumService albumService,
            IClientService clientService, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, IImageService imageService, IFileService fileService,
            ApplicationDbContext context)
        {
            _photoService = photoService;
            _albumService = albumService;
            _clientService = clientService;
            _userManager = userManager;
            _environment = environment;
            _imageService = imageService;
            _fileService = fileService;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

                const long maxBytes = 20L * 1024 * 1024; // 20 MB per file

                // Build naming components from photoshoot data
                var photoShoot = album.PhotoShoot;
                var clientName = photoShoot?.ClientProfile?.User != null
                    ? $"{photoShoot.ClientProfile.User.FirstName}_{photoShoot.ClientProfile.User.LastName}"
                    : "Client";
                var shootName = SanitizeFileName(photoShoot?.Title ?? "Photoshoot");
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
                    if (!IsImageFile(file))
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
                            ClientProfileId = album.PhotoShoot.ClientProfileId,
                            UploadDate = DateTime.Now,
                            UploadedDate = DateTime.Now,
                            DisplayOrder = 0,
                            IsSelected = false
                        };

                        await _photoService.CreatePhotoAsync(photo);

                        // Copy photo to client's folder in File Manager
                        var clientProfile = album.PhotoShoot?.ClientProfile;
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

            // Admins and Photographers can access all photos
            if (!User.IsInRole("Admin") && !User.IsInRole("Photographer"))
            {
                // For clients or other users, verify they own the photo
                var userId = _userManager.GetUserId(User);
                var client = await _clientService.GetClientByUserIdAsync(userId!);
                if (client == null || photo.Album?.PhotoShoot?.ClientProfileId != client.Id)
                {
                    return Forbid();
                }
            }

            // Convert relative path to absolute if needed
            var absolutePath = GetAbsolutePath(photo.FilePath);

            // Return the photo file
            if (System.IO.File.Exists(absolutePath))
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var mimeType = GetMimeType(absolutePath);
                return File(memory, mimeType);
            }

            return NotFound();
        }

        /// <summary>
        /// Returns the full-size image file for lightbox display.
        /// Similar to View but with AllowAnonymous for proper lightbox loading.
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Image(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            // Allow access if photo is in a public gallery or user has permission
            var isInPublicGallery = photo.Album?.Galleries?.Any(g => g.IsActive && g.ExpiryDate > DateTime.UtcNow) ?? false;

            // If not in a public gallery, check user permissions
            if (!isInPublicGallery && User.Identity?.IsAuthenticated == true)
            {
                // Admins and Photographers can access all photos
                if (!User.IsInRole("Admin") && !User.IsInRole("Photographer"))
                {
                    // For clients or other users, verify they own the photo
                    var userId = _userManager.GetUserId(User);
                    var client = await _clientService.GetClientByUserIdAsync(userId!);
                    if (client == null || photo.Album?.PhotoShoot?.ClientProfileId != client.Id)
                    {
                        return Forbid();
                    }
                }
            }
            else if (!isInPublicGallery && User.Identity?.IsAuthenticated != true)
            {
                // Not in public gallery and not authenticated
                return Forbid();
            }

            // Convert relative path to absolute if needed
            var absolutePath = GetAbsolutePath(photo.FilePath);

            // Return the photo file
            if (System.IO.File.Exists(absolutePath))
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var mimeType = GetMimeType(absolutePath);
                return File(memory, mimeType);
            }

            return NotFound();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Thumbnail(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            // Allow access if photo is in a public gallery or user has permission
            // Check if this photo belongs to an active gallery (for public access)
            var isInPublicGallery = photo.Album?.Galleries?.Any(g => g.IsActive && g.ExpiryDate > DateTime.UtcNow) ?? false;

            // If not in a public gallery, check user permissions
            if (!isInPublicGallery && User.Identity?.IsAuthenticated == true)
            {
                // Admins and Photographers can access all photos
                if (!User.IsInRole("Admin") && !User.IsInRole("Photographer"))
                {
                    // For clients or other users, verify they own the photo
                    var userId = _userManager.GetUserId(User);
                    var client = await _clientService.GetClientByUserIdAsync(userId!);
                    if (client == null || photo.Album?.PhotoShoot?.ClientProfileId != client.Id)
                    {
                        return Forbid();
                    }
                }
            }
            else if (!isInPublicGallery && User.Identity?.IsAuthenticated != true)
            {
                // Not in public gallery and not authenticated
                return Forbid();
            }

            // Convert relative paths to absolute if needed
            var absoluteThumbPath = !string.IsNullOrEmpty(photo.ThumbnailPath) ? GetAbsolutePath(photo.ThumbnailPath) : null;
            var absoluteFilePath = GetAbsolutePath(photo.FilePath);

            // Return the thumbnail file, fall back to full image if thumbnail doesn't exist
            var thumbnailPath = (!string.IsNullOrEmpty(absoluteThumbPath) && System.IO.File.Exists(absoluteThumbPath))
                ? absoluteThumbPath
                : absoluteFilePath;

            if (System.IO.File.Exists(thumbnailPath))
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var mimeType = GetMimeType(thumbnailPath);
                return File(memory, mimeType);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        private bool IsImageFile(IFormFile file)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }

        /// <summary>
        /// Sanitizes a string for use as a filename by removing invalid characters and spaces
        /// </summary>
        private static string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name
                .Where(c => !invalidChars.Contains(c))
                .Select(c => c == ' ' ? '_' : c) // Replace spaces with underscores
                .ToArray());
            return sanitized.Trim('_');
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Converts relative web path to absolute server path
        /// </summary>
        private string GetAbsolutePath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // If already absolute, return as-is
            if (Path.IsPathRooted(path) && !path.StartsWith('/'))
                return path;

            // Convert relative web path (e.g., /uploads/albums/1/xyz.jpg) to absolute server path
            if (path.StartsWith('/'))
            {
                var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                return Path.Combine(webRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            return path;
        }
    }
}