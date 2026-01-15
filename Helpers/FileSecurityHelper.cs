namespace MyPhotoBiz.Helpers
{
    /// <summary>
    /// Security helper for file path validation and protection against path traversal attacks
    /// </summary>
    public static class FileSecurityHelper
    {
        /// <summary>
        /// Validates that a file path is within the wwwroot directory and doesn't contain path traversal attempts
        /// </summary>
        /// <param name="filePath">The file path to validate (can be relative or absolute)</param>
        /// <param name="logger">Logger for security warnings</param>
        /// <returns>True if the path is safe, false otherwise</returns>
        public static bool IsPathSafeForWwwroot(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                logger?.LogWarning("Attempted to validate empty file path");
                return false;
            }

            try
            {
                // Get the full wwwroot path
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullWwwrootPath = Path.GetFullPath(wwwrootPath);

                // Build the full file path
                var fullFilePath = filePath.StartsWith(wwwrootPath) || Path.IsPathFullyQualified(filePath)
                    ? Path.GetFullPath(filePath)
                    : Path.GetFullPath(Path.Combine(wwwrootPath, filePath.TrimStart('/', '\\')));

                // Check if the resolved path is within wwwroot
                if (!fullFilePath.StartsWith(fullWwwrootPath, StringComparison.OrdinalIgnoreCase))
                {
                    logger?.LogWarning("Path traversal attempt detected: {FilePath} resolved to {FullFilePath}",
                        filePath, fullFilePath);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error validating file path: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Gets the safe full path for a file within wwwroot, or null if the path is unsafe
        /// </summary>
        /// <param name="relativePath">The relative path within wwwroot</param>
        /// <param name="logger">Logger for security warnings</param>
        /// <returns>The full safe path, or null if validation fails</returns>
        public static string? GetSafeWwwrootPath(string relativePath, ILogger? logger = null)
        {
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(wwwrootPath, relativePath.TrimStart('/', '\\'));

            return IsPathSafeForWwwroot(fullPath, logger) ? fullPath : null;
        }

        /// <summary>
        /// Sanitizes a filename by removing invalid characters and limiting length
        /// </summary>
        /// <param name="fileName">The filename to sanitize</param>
        /// <param name="maxLength">Maximum allowed length (default: 255)</param>
        /// <returns>A sanitized filename safe for file system operations</returns>
        public static string SanitizeFileName(string fileName, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "unnamed_file";

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Remove leading/trailing whitespace and dots (security risk on Windows)
            sanitized = sanitized.Trim().TrimStart('.');

            // Limit length
            if (sanitized.Length > maxLength)
            {
                var extension = Path.GetExtension(sanitized);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
                var maxNameLength = maxLength - extension.Length;
                sanitized = nameWithoutExt.Substring(0, Math.Min(nameWithoutExt.Length, maxNameLength)) + extension;
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "unnamed_file" : sanitized;
        }
    }
}
