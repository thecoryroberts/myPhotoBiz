using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyPhotoBiz.Services;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Represents view model data for edit gallery.
    /// </summary>
    public class EditGalleryViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Must be a valid hex color")]
        [Display(Name = "Brand Color")]
        public string BrandColor { get; set; } = "#2c3e50";

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        // Watermark Settings
        [Display(Name = "Enable Watermark")]
        public bool WatermarkEnabled { get; set; }

        [StringLength(100)]
        [Display(Name = "Watermark Text")]
        public string? WatermarkText { get; set; }

        [StringLength(500)]
        [Display(Name = "Watermark Image Path")]
        public string? WatermarkImagePath { get; set; }

        [Range(0.1, 1.0)]
        [Display(Name = "Watermark Opacity")]
        public float WatermarkOpacity { get; set; } = 0.5f;

        [Display(Name = "Watermark Position")]
        public WatermarkPosition WatermarkPosition { get; set; } = WatermarkPosition.Center;

        [Display(Name = "Tiled Watermark")]
        public bool WatermarkTiled { get; set; }

        // Album management
        public List<int> SelectedAlbumIds { get; set; } = new();
        public List<AlbumSelectionViewModel> AvailableAlbums { get; set; } = new();

        // Client access management
        public List<int> SelectedClientProfileIds { get; set; } = new();
        public List<ClientSelectionViewModel> AvailableClients { get; set; } = new();
    }
}
