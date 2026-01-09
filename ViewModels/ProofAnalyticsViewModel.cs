using System.Collections.Generic;

namespace MyPhotoBiz.ViewModels
{
    public class ProofAnalyticsViewModel
    {
        // Top Photos
        public List<PopularPhotoViewModel> MostFavoritedPhotos { get; set; } = new();
        public List<PopularPhotoViewModel> MostEditRequested { get; set; } = new();

        // Gallery Performance
        public List<GalleryProofStatsViewModel> GalleryStats { get; set; } = new();

        // Time-based Analytics
        public Dictionary<string, int> ProofsByDate { get; set; } = new();
        public Dictionary<string, int> ProofsByGallery { get; set; } = new();
    }

    public class PopularPhotoViewModel
    {
        public int PhotoId { get; set; }
        public string PhotoTitle { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<string> GalleryNames { get; set; } = new();
    }

    public class GalleryProofStatsViewModel
    {
        public int GalleryId { get; set; }
        public string GalleryName { get; set; } = string.Empty;
        public int TotalProofs { get; set; }
        public int Favorites { get; set; }
        public int EditRequests { get; set; }
        public int TotalPhotos { get; set; }
        public double EngagementRate => TotalPhotos > 0 ? (double)TotalProofs / TotalPhotos : 0;
    }
}
