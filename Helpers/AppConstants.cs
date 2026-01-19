namespace MyPhotoBiz.Helpers
{
    /// <summary>
    /// Centralized constants for the application to avoid magic numbers and strings
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// File size limits
        /// </summary>
        public static class FileSizes
        {
            public const long MaxPhotoUploadBytes = 20L * 1024 * 1024; // 20 MB
            public const long MaxDocumentUploadBytes = 10L * 1024 * 1024; // 10 MB
        }

        /// <summary>
        /// Pagination defaults
        /// </summary>
        public static class Pagination
        {
            public const int DefaultPageSize = 48;
            public const int SmallPageSize = 10;
            public const int LargePageSize = 100;
        }

        /// <summary>
        /// Cache durations
        /// </summary>
        public static class Cache
        {
            public const int DashboardCacheMinutes = 5;
            public const int GalleryCacheMinutes = 10;
        }

        /// <summary>
        /// File type categories for filtering
        /// </summary>
        public static class FileTypes
        {
            public static readonly string[] ImageExtensions = { "JPG", "JPEG", "PNG", "GIF", "BMP", "WEBP" };
            public static readonly string[] DocumentExtensions = { "PDF", "DOC", "DOCX", "ODT", "TXT", "RTF" };
            public static readonly string[] VideoExtensions = { "MP4", "AVI", "MOV", "WMV", "MKV" };
            public static readonly string[] ArchiveExtensions = { "ZIP", "RAR", "7Z", "TAR", "GZ" };

            public static readonly string[] ImageMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        }

        /// <summary>
        /// User roles
        /// </summary>
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Photographer = "Photographer";
            public const string Client = "Client";
            public static readonly string[] StaffRoles = { Admin, Photographer };
        }
    }
}
