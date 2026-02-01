using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the gallery service contract.
    /// </summary>
    public interface IGalleryService
    {
        // CRUD Operations
        Task<IEnumerable<GalleryListItemViewModel>> GetAllGalleriesAsync();
        Task<GalleryDetailsViewModel?> GetGalleryDetailsAsync(int id, int page = 1, int pageSize = 50);
        Task<Gallery?> GetGalleryByIdAsync(int id);
        Task<Gallery> CreateGalleryAsync(CreateGalleryViewModel model);
        Task<Gallery> UpdateGalleryAsync(EditGalleryViewModel model);
        Task<bool> DeleteGalleryAsync(int id);
        Task<bool> ToggleGalleryStatusAsync(int id, bool isActive);

        // Access Management (Identity-based)
        Task<GalleryAccess> GrantAccessAsync(
            int galleryId,
            int clientProfileId,
            DateTime? expiryDate = null,
            bool canDownload = true,
            bool canProof = true,
            bool canOrder = true);
        Task<bool> RevokeAccessAsync(int galleryId, int clientProfileId);
        Task<bool> ValidateUserAccessAsync(int galleryId, string userId);
        Task<IEnumerable<GalleryAccess>> GetGalleryAccessesAsync(int galleryId);

        // Public Access (Token-based, no login required)
        Task<string> EnablePublicAccessAsync(int galleryId);
        Task<bool> DisablePublicAccessAsync(int galleryId);
        Task<Gallery?> GetGalleryByPublicTokenAsync(string token);
        Task<int?> GetGalleryIdByTokenAsync(string token);
        Task<bool> ValidatePublicAccessAsync(int galleryId, string token);

        // Album Management
        Task<bool> AddAlbumsToGalleryAsync(int galleryId, List<int> albumIds);
        Task<bool> RemoveAlbumsFromGalleryAsync(int galleryId, List<int> albumIds);
        Task<List<AlbumSelectionViewModel>> GetAvailableAlbumsAsync(int? currentGalleryId = null);

        // Client Management
        Task<List<ClientSelectionViewModel>> GetAvailableClientsAsync();

        // Session Management
        Task<IEnumerable<GallerySessionViewModel>> GetGallerySessionsAsync(int galleryId);
        Task<bool> EndSessionAsync(int sessionId);
        Task<bool> EndAllSessionsAsync(int galleryId);

        // Analytics & Logging
        Task<GalleryStatsSummaryViewModel> GetGalleryStatsAsync();
        Task<string> GetGalleryAccessUrlAsync(int galleryId, string baseUrl);
        Task LogDownloadAsync(int galleryId, int photoId, string? userId, string? ipAddress);

        // Client/Viewer Experiences
        Task<ClientGalleryIndexResult> GetAccessibleGalleriesForUserAsync(string userId);
        Task<GalleryViewPageResult?> GetGalleryViewPageForUserAsync(int galleryId, string userId, int page, int pageSize);
        Task<GalleryViewPageResult?> GetPublicGalleryViewPageByTokenAsync(string token, int page, int pageSize);
        Task<GalleryViewPageResult?> GetPublicGalleryViewPageBySlugAsync(string slug, int page, int pageSize);
        Task<GalleryPhotosPageResult?> GetGalleryPhotosPageAsync(int galleryId, int page, int pageSize);
        Task<GallerySessionInfoResult?> GetGallerySessionInfoAsync(int galleryId, string userId);
        Task<bool> EndGallerySessionAsync(int galleryId, string userId);
        Task<GalleryPhotoDownloadResult> GetPhotoDownloadAsync(int galleryId, int photoId, string userId);
        Task<GalleryBulkDownloadResult> GetBulkDownloadAsync(int galleryId, List<int> photoIds, string userId);
    }
}
