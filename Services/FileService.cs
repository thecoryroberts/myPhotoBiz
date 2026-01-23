using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing file and folder operations including uploads, downloads, metadata, and organization
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FileService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Retrieves files with optional category/type filtering and pagination
        /// </summary>
        public async Task<IEnumerable<FileItem>> GetFilesAsync(string filterType, int page, int pageSize)
        {
            var query = ApplyFileTypeFilter(_context.Files.AsQueryable(), filterType);

            return await query
                .OrderByDescending(f => f.Modified)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Applies file type filter to a query based on category or specific extension
        /// </summary>
        private static IQueryable<FileItem> ApplyFileTypeFilter(IQueryable<FileItem> query, string? filterType)
        {
            if (string.IsNullOrEmpty(filterType))
                return query;

            var filter = filterType.ToLower();
            var extensions = FileHelper.GetExtensionsForCategory(filter);

            if (extensions.Length > 0)
            {
                return query.Where(f => extensions.Contains(f.Type));
            }

            // Handle special filters
            if (filter == "favorites")
            {
                return query.Where(f => f.IsFavorite);
            }

            // Exact type filter
            var filterTypeUpper = filterType.ToUpper();
            return query.Where(f => f.Type.ToUpper() == filterTypeUpper);
        }

        public async Task<FileItem?> GetFileAsync(int id) => await _context.Files.FindAsync(id);

        public async Task UploadFileAsync(IFormFile file, string owner)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var originalExtension = Path.GetExtension(file.FileName);
            var fileName = Path.GetFileName(file.FileName);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                var fileNameChars = fileName.ToCharArray();
                for (var i = 0; i < fileNameChars.Length; i++)
                {
                    if (Array.IndexOf(invalidChars, fileNameChars[i]) >= 0)
                    {
                        fileNameChars[i] = '_';
                    }
                }
                fileName = new string(fileNameChars).Trim();
            }
            if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName)))
            {
                fileName = $"upload_{Guid.NewGuid():N}{originalExtension}";
            }
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var filePath = Path.Combine(uploadPath, fileName);
            while (System.IO.File.Exists(filePath))
            {
                fileName = $"{baseName}_{Guid.NewGuid():N}{extension}";
                filePath = Path.Combine(uploadPath, fileName);
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileItem = new FileItem
            {
                Name = fileName,
                Type = Path.GetExtension(fileName).Trim('.').ToUpper(),
                Size = file.Length,
                Modified = DateTime.Now,
                Owner = owner,
                FilePath = filePath
            };

            _context.Files.Add(fileItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFileAsync(int id)
        {
            var fileItem = await _context.Files.Include(f => f.Children).FirstOrDefaultAsync(f => f.Id == id);
            if (fileItem != null)
            {
                // If it's a folder, delete all children recursively
                if (fileItem.IsFolder)
                {
                    await DeleteFolderRecursiveAsync(fileItem);
                }
                else
                {
                    // Delete physical file
                    if (!string.IsNullOrEmpty(fileItem.FilePath) && System.IO.File.Exists(fileItem.FilePath))
                        System.IO.File.Delete(fileItem.FilePath);
                }

                _context.Files.Remove(fileItem);
                await _context.SaveChangesAsync();
            }
        }

        private async Task DeleteFolderRecursiveAsync(FileItem folder)
        {
            var children = await _context.Files.Where(f => f.ParentFolderId == folder.Id).ToListAsync();
            foreach (var child in children)
            {
                if (child.IsFolder)
                {
                    await DeleteFolderRecursiveAsync(child);
                }
                else
                {
                    if (!string.IsNullOrEmpty(child.FilePath) && System.IO.File.Exists(child.FilePath))
                        System.IO.File.Delete(child.FilePath);
                }
                _context.Files.Remove(child);
            }

            // Delete folder's physical directory if it exists
            if (!string.IsNullOrEmpty(folder.FilePath) && Directory.Exists(folder.FilePath))
            {
                Directory.Delete(folder.FilePath, true);
            }
        }

        #region Folder Methods

        public async Task<IEnumerable<FileItem>> GetFilesInFolderAsync(int? folderId, string filterType, int page, int pageSize)
        {
            var query = _context.Files.Where(f => f.ParentFolderId == folderId);
            query = ApplyFileTypeFilter(query, filterType);

            // Folders first, then files
            return await query
                .OrderByDescending(f => f.IsFolder)
                .ThenByDescending(f => f.Modified)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<FileItem> CreateFolderAsync(string folderName, string owner, int? parentFolderId = null)
        {
            // Get parent folder path
            string parentPath = Path.Combine(_env.WebRootPath, "uploads");
            if (parentFolderId.HasValue)
            {
                var parentFolder = await _context.Files.FindAsync(parentFolderId.Value);
                if (parentFolder != null && !string.IsNullOrEmpty(parentFolder.FilePath))
                {
                    parentPath = parentFolder.FilePath;
                }
            }

            // Create physical folder
            var folderPath = Path.Combine(parentPath, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create database entry
            var folder = new FileItem
            {
                Name = folderName,
                Type = "FOLDER",
                Size = 0,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Owner = owner,
                FilePath = folderPath,
                IsFolder = true,
                ParentFolderId = parentFolderId
            };

            _context.Files.Add(folder);
            await _context.SaveChangesAsync();

            return folder;
        }

        public async Task<IEnumerable<FileItem>> GetBreadcrumbsAsync(int? folderId)
        {
            var breadcrumbs = new List<FileItem>();

            if (!folderId.HasValue)
                return breadcrumbs;

            var currentFolder = await _context.Files.FindAsync(folderId.Value);
            while (currentFolder != null)
            {
                breadcrumbs.Insert(0, currentFolder);
                if (currentFolder.ParentFolderId.HasValue)
                {
                    currentFolder = await _context.Files.FindAsync(currentFolder.ParentFolderId.Value);
                }
                else
                {
                    break;
                }
            }

            return breadcrumbs;
        }

        public async Task<bool> IsFolderEmptyAsync(int folderId)
        {
            return !await _context.Files.AnyAsync(f => f.ParentFolderId == folderId);
        }

        #endregion

        #region Metadata Methods

        public async Task UpdateMetadataAsync(int fileId, string? description, string? tags, bool? isFavorite)
        {
            var fileItem = await _context.Files.FindAsync(fileId);
            if (fileItem != null)
            {
                if (description != null)
                    fileItem.Description = description;

                if (tags != null)
                    fileItem.Tags = tags;

                if (isFavorite.HasValue)
                    fileItem.IsFavorite = isFavorite.Value;

                fileItem.Modified = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FileItem>> GetFavoritesAsync(string owner, int page, int pageSize)
        {
            return await _context.Files
                .Where(f => f.Owner == owner && f.IsFavorite)
                .OrderByDescending(f => f.Modified)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FileItem>> GetRecentFilesAsync(string owner, int page, int pageSize)
        {
            return await _context.Files
                .Where(f => f.Owner == owner && !f.IsFolder)
                .OrderByDescending(f => f.LastAccessed ?? f.Modified)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task IncrementDownloadCountAsync(int fileId)
        {
            var fileItem = await _context.Files.FindAsync(fileId);
            if (fileItem != null)
            {
                fileItem.DownloadCount++;
                fileItem.LastAccessed = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Bulk Upload

        public async Task UploadFilesAsync(IFormFileCollection files, string owner, int? parentFolderId = null)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            // If uploading to a specific folder
            if (parentFolderId.HasValue)
            {
                var parentFolder = await _context.Files.FindAsync(parentFolderId.Value);
                if (parentFolder != null && !string.IsNullOrEmpty(parentFolder.FilePath))
                {
                    uploadPath = parentFolder.FilePath;
                }
            }

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                // Extract folder structure from file name (if webkitRelativePath exists)
                var relativePath = file.FileName;
                var fileName = Path.GetFileName(relativePath);
                var directoryPath = Path.GetDirectoryName(relativePath);

                // Create subdirectories if needed
                int? currentParentId = parentFolderId;
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    var folders = directoryPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var folderName in folders)
                    {
                        // Check if folder already exists
                        var existingFolder = await _context.Files
                            .FirstOrDefaultAsync(f => f.Name == folderName && f.IsFolder && f.ParentFolderId == currentParentId);

                        if (existingFolder == null)
                        {
                            existingFolder = await CreateFolderAsync(folderName, owner, currentParentId);
                        }

                        currentParentId = existingFolder.Id;
                        uploadPath = existingFolder.FilePath!;
                    }
                }

                // Upload the file
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileItem = new FileItem
                {
                    Name = fileName,
                    Type = Path.GetExtension(fileName).Trim('.').ToUpper(),
                    Size = file.Length,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    Owner = owner,
                    FilePath = filePath,
                    IsFolder = false,
                    ParentFolderId = currentParentId,
                    MimeType = file.ContentType
                };

                _context.Files.Add(fileItem);
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Client Folder Management

        /// <summary>
        /// Gets the root "Clients" folder that contains all client folders
        /// </summary>
        public async Task<FileItem?> GetClientsFolderAsync()
        {
            return await _context.Files
                .FirstOrDefaultAsync(f => f.Name == "Clients" && f.IsFolder && f.ParentFolderId == null);
        }

        /// <summary>
        /// Creates a folder for a new client under the "Clients" root folder
        /// </summary>
        public async Task<FileItem> CreateClientFolderAsync(string clientName, string owner)
        {
            // Get or create the root "Clients" folder
            var clientsFolder = await GetClientsFolderAsync() ?? await CreateFolderAsync("Clients", owner, null);

            // Sanitize client name for use as folder name
            var sanitizedName = FileSecurityHelper.SanitizeFileName(clientName);

            // Check if folder already exists for this client
            var existingFolder = await _context.Files
                .FirstOrDefaultAsync(f => f.Name == sanitizedName && f.IsFolder && f.ParentFolderId == clientsFolder.Id);

            return existingFolder ?? await CreateFolderAsync(sanitizedName, owner, clientsFolder.Id);
        }

        /// <summary>
        /// Copies a photo from the album uploads to the client's folder in the file manager
        /// </summary>
        public async Task CopyPhotoToClientFolderAsync(int clientFolderId, string sourcePath, string fileName, string owner)
        {
            var clientFolder = await _context.Files.FindAsync(clientFolderId);
            if (clientFolder == null || !clientFolder.IsFolder)
                return;

            // Get or create a "Photos" subfolder within the client folder
            var photosFolder = await _context.Files
                .FirstOrDefaultAsync(f => f.Name == "Photos" && f.IsFolder && f.ParentFolderId == clientFolderId)
                ?? await CreateFolderAsync("Photos", owner, clientFolderId);

            // Determine destination path with unique name
            var destPath = Path.Combine(photosFolder.FilePath!, fileName);
            fileName = EnsureUniqueFileName(ref destPath, fileName, photosFolder.FilePath!);

            // Copy the file
            if (!System.IO.File.Exists(sourcePath))
                return;

            System.IO.File.Copy(sourcePath, destPath, false);

            // Create database entry
            var fileInfo = new System.IO.FileInfo(destPath);
            var fileItem = new FileItem
            {
                Name = fileName,
                Type = Path.GetExtension(fileName).Trim('.').ToUpper(),
                Size = fileInfo.Length,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Owner = owner,
                FilePath = destPath,
                IsFolder = false,
                ParentFolderId = photosFolder.Id,
                MimeType = FileHelper.GetMimeType(fileName)
            };

            _context.Files.Add(fileItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ensures file name is unique in directory, modifying destPath and returning the final fileName
        /// </summary>
        private static string EnsureUniqueFileName(ref string destPath, string fileName, string directory)
        {
            if (!System.IO.File.Exists(destPath))
                return fileName;

            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var counter = 1;

            while (System.IO.File.Exists(destPath))
            {
                fileName = $"{nameWithoutExt}_{counter}{ext}";
                destPath = Path.Combine(directory, fileName);
                counter++;
            }

            return fileName;
        }

        #endregion
    }
}
