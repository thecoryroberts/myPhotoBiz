using MyPhotoBiz.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPhotoBiz.Services
{
    public interface IProofService
    {
        // Query Operations
        Task<IEnumerable<ProofListItemViewModel>> GetAllProofsAsync(ProofFilterViewModel? filter = null);
        Task<ProofDetailsViewModel?> GetProofDetailsAsync(int id);
        Task<IEnumerable<ProofListItemViewModel>> GetProofsByGalleryAsync(int galleryId);
        Task<IEnumerable<ProofListItemViewModel>> GetProofsByPhotoAsync(int photoId);

        // Analytics
        Task<ProofStatsSummaryViewModel> GetProofStatsAsync();
        Task<ProofAnalyticsViewModel> GetProofAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<PopularPhotoViewModel>> GetMostFavoritedPhotosAsync(int topN = 10);
        Task<List<PopularPhotoViewModel>> GetMostEditRequestedPhotosAsync(int topN = 10);

        // Export Operations (simplified - return CSV string for now)
        Task<string> ExportProofsToCsvAsync(ProofFilterViewModel? filter = null);

        // Management Operations
        Task<bool> DeleteProofAsync(int id);
        Task<bool> BulkDeleteProofsAsync(List<int> ids);

        // Helper
        Task<List<GalleryFilterOption>> GetGalleryFilterOptionsAsync();
    }
}
