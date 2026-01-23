using System;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Represents view model data for proof details.
    /// </summary>
    public class ProofDetailsViewModel
    {
        public int Id { get; set; }

        // Photo Info
        public int PhotoId { get; set; }
        public string PhotoTitle { get; set; } = string.Empty;
        public string PhotoThumbnailPath { get; set; } = string.Empty;
        public string PhotoFullImagePath { get; set; } = string.Empty;

        // Gallery Info
        public int GalleryId { get; set; }
        public string GalleryName { get; set; } = string.Empty;
        public string GalleryDescription { get; set; } = string.Empty;

        // Session Info
        public int? GallerySessionId { get; set; }
        public string? SessionToken { get; set; }
        public DateTime? SessionCreatedDate { get; set; }
        public DateTime? SessionLastAccessDate { get; set; }

        // Client Info
        public string? ClientName { get; set; }

        // Proof Details
        public bool IsFavorite { get; set; }
        public bool IsMarkedForEditing { get; set; }
        public string? EditingNotes { get; set; }
        public DateTime SelectedDate { get; set; }
    }
}
