using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
    public class CreateGalleryViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Gallery Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddMonths(3);

        [Display(Name = "Brand Color")]
        [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Must be a valid hex color")]
        public string BrandColor { get; set; } = "#2c3e50";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Auto-generate Access Code")]
        public bool AutoGenerateCode { get; set; } = true;

        [StringLength(50)]
        [Display(Name = "Client Code (leave blank to auto-generate)")]
        public string? ClientCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Client Password (leave blank to auto-generate)")]
        public string? ClientPassword { get; set; }

        [Display(Name = "Select Photos")]
        public List<int> SelectedPhotoIds { get; set; } = new();

        public List<PhotoSelectionViewModel> AvailablePhotos { get; set; } = new();
    }

    public class PhotoSelectionViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? AlbumId { get; set; }
        public string? AlbumName { get; set; }
        public bool IsSelected { get; set; }
    }
}
