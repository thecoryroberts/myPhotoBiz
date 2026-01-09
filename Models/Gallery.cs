using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MyPhotoBiz.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Access is controlled via GalleryAccess records linked to ClientProfile
        // OR via public access token for sharing without login

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string BrandColor { get; set; } = "#2c3e50";
        public string? LogoPath { get; set; }

        // Public access token for sharing galleries without requiring login
        // Set to null to disable public access
        public string? PublicAccessToken { get; set; }
        public bool AllowPublicAccess { get; set; } = false;

        // Slug for SEO-friendly URLs (e.g., /gallery/smith-wedding-2024)
        public string? Slug { get; set; }

        // Navigation properties
        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
        public virtual ICollection<GallerySession> Sessions { get; set; } = new List<GallerySession>();

        // Access control via authenticated users
        public virtual ICollection<GalleryAccess> Accesses { get; set; } = new List<GalleryAccess>();

        /// <summary>
        /// Generates a new public access token for this gallery
        /// </summary>
        public string GeneratePublicAccessToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            PublicAccessToken = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
            return PublicAccessToken;
        }
    }
}