
// Models/Photo.cs
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    public class Photo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? FileName { get; set; } = string.Empty;

        [Required]
        public string? FilePath { get; set; } = string.Empty;

        public string? ThumbnailPath { get; set; }

        public long FileSize { get; set; }

        public string? Description { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public bool IsSelected { get; set; } = false;

        // Foreign key
        public int AlbumId { get; set; }
        public virtual Album Album { get; set; } = null!;

        public int GalleryId { get; set; }
        public string? Title { get; set; } = string.Empty;

        public string? FullImagePath { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UploadedDate { get; set; }

        public Gallery? Gallery { get; set; }
        public ICollection<Proof> Proofs { get; set; } = new List<Proof>();
    }
}
