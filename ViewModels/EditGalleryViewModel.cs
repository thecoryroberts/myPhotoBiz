using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
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

        // Display only (not editable from this form)
        public string ClientCode { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // Photo management
        public List<int> SelectedPhotoIds { get; set; } = new();
        public List<PhotoSelectionViewModel> AvailablePhotos { get; set; } = new();
    }
}
