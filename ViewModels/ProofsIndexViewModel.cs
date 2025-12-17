using System;
using System.Collections.Generic;

namespace MyPhotoBiz.ViewModels
{
    public class ProofsIndexViewModel
    {
        public List<ProofListItemViewModel> Proofs { get; set; } = new();
        public ProofFilterViewModel Filters { get; set; } = new();
        public ProofStatsSummaryViewModel Stats { get; set; } = new();
        public List<GalleryFilterOption> AvailableGalleries { get; set; } = new();
    }

    public class ProofListItemViewModel
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public string PhotoTitle { get; set; } = string.Empty;
        public string PhotoThumbnailPath { get; set; } = string.Empty;

        // Gallery Info
        public int GalleryId { get; set; }
        public string GalleryName { get; set; } = string.Empty;

        // Session Info
        public int? GallerySessionId { get; set; }
        public string? ClientName { get; set; }
        public DateTime SessionCreatedDate { get; set; }

        // Proof Details
        public bool IsFavorite { get; set; }
        public bool IsMarkedForEditing { get; set; }
        public string? EditingNotes { get; set; }
        public DateTime SelectedDate { get; set; }

        // Computed
        public string ProofType
        {
            get
            {
                if (IsFavorite && IsMarkedForEditing)
                    return "Both";
                if (IsFavorite)
                    return "Favorite";
                if (IsMarkedForEditing)
                    return "Edit Request";
                return "Unknown";
            }
        }
    }

    public class ProofFilterViewModel
    {
        public int? GalleryId { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? IsMarkedForEditing { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class ProofStatsSummaryViewModel
    {
        public int TotalProofs { get; set; }
        public int TotalFavorites { get; set; }
        public int TotalEditingRequests { get; set; }
        public int UniquePhotosMarked { get; set; }
        public int ActiveGalleriesWithProofs { get; set; }
    }

    public class GalleryFilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
