// Models/Photo.cs
namespace MyPhotoBiz.Models
{
     public class Gallery
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ClientCode { get; set; }
        public required string ClientPassword { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string BrandColor { get; set; } = "#2c3e50";
        public required string LogoPath { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public ICollection<GallerySession> Sessions { get; set; } = new List<GallerySession>();
    }
}