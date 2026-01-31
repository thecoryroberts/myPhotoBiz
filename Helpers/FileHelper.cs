namespace MyPhotoBiz.Helpers
{
    /// <summary>
    /// Centralized file operations helper to eliminate duplicate file handling code
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Checks if a file is an allowed image type based on MIME type
        /// </summary>
        public static bool IsImageFile(IFormFile file)
        {
            return AppConstants.FileTypes.ImageMimeTypes.Contains(file.ContentType.ToLower());
        }

        /// <summary>
        /// Checks if a file extension is an image type
        /// </summary>
        public static bool IsImageExtension(string extension)
        {
            var ext = extension.TrimStart('.').ToUpperInvariant();
            return AppConstants.FileTypes.ImageExtensions.Contains(ext);
        }

        /// <summary>
        /// Gets the MIME type for a file based on its extension
        /// </summary>
        public static string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".mp4" => "video/mp4",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Converts relative web path to absolute server path
        /// </summary>
        public static string GetAbsolutePath(string? path, string webRootPath)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // If already absolute, return as-is
            if (Path.IsPathRooted(path) && !path.StartsWith('/'))
                return path;

            // Convert relative web path (e.g., /uploads/albums/1/xyz.jpg) to absolute server path
            if (path.StartsWith('/'))
            {
                return Path.Combine(webRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            return path;
        }

        /// <summary>
        /// Sanitizes a string for use as a filename by removing invalid characters
        /// </summary>
        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "unnamed";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name
                .Where(c => !invalidChars.Contains(c))
                .Select(c => c == ' ' ? '_' : c)
                .ToArray());
            return sanitized.Trim('_');
        }

        /// <summary>
        /// Gets file category based on extension for filtering
        /// </summary>
        public static string GetFileCategory(string extension)
        {
            var ext = extension.TrimStart('.').ToUpperInvariant();

            if (AppConstants.FileTypes.ImageExtensions.Contains(ext))
                return "images";
            if (AppConstants.FileTypes.DocumentExtensions.Contains(ext))
                return "documents";
            if (AppConstants.FileTypes.VideoExtensions.Contains(ext))
                return "videos";
            if (AppConstants.FileTypes.ArchiveExtensions.Contains(ext))
                return "archives";

            return "other";
        }

        /// <summary>
        /// Builds a file query filter expression based on category
        /// </summary>
        public static string[] GetExtensionsForCategory(string category)
        {
            return category.ToLower() switch
            {
                "images" => AppConstants.FileTypes.ImageExtensions,
                "documents" => AppConstants.FileTypes.DocumentExtensions,
                "videos" => AppConstants.FileTypes.VideoExtensions,
                "archives" => AppConstants.FileTypes.ArchiveExtensions,
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// Reads file into memory stream for serving
        /// </summary>
        public static async Task<MemoryStream?> ReadFileToMemoryAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var memory = new MemoryStream();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await stream.CopyToAsync(memory);
            memory.Position = 0;
            return memory;
        }

        /// <summary>
        /// Deletes a file if it exists (no-op for null/empty paths)
        /// </summary>
        public static void DeleteFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
