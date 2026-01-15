using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    public class FileItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Type { get; set; }

        public long Size { get; set; } // in bytes

        public DateTime Modified { get; set; }

        public DateTime Created { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Owner { get; set; }

        public List<string> SharedWith { get; set; } = new();

        [MaxLength(500)]
        public string? FilePath { get; set; }

        // Folder support
        public bool IsFolder { get; set; }

        public int? ParentFolderId { get; set; }

        public FileItem? ParentFolder { get; set; }

        public ICollection<FileItem> Children { get; set; } = new List<FileItem>();

        // Metadata fields
        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        public bool IsFavorite { get; set; }

        public DateTime? LastAccessed { get; set; }

        public int DownloadCount { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        // Many-to-many relationship with Tag entity (if needed for advanced tagging)
        public ICollection<FileItemTag> FileItemTags { get; set; } = new List<FileItemTag>();
    }
}

namespace MyPhotoBiz.Models
{
    // The joining entity (explicit for full control, or implicit in newer EF Core versions)
    public class FileItemTag
    {
        public int Id { get; set; }
        public int FileItemId { get; set; }
        public required FileItem FileItem { get; set; }

        public int TagId { get; set; }
        public required Tag Tag { get; set; }
    }
}