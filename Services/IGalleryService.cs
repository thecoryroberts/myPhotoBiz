using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPhotoBiz.Services
{
    public interface IGalleryService
    {
        // CRUD Operations
        Task<IEnumerable<GalleryListItemViewModel>> GetAllGalleriesAsync();
        Task<GalleryDetailsViewModel?> GetGalleryDetailsAsync(int id);
        Task<Gallery?> GetGalleryByIdAsync(int id);
        Task<Gallery> CreateGalleryAsync(CreateGalleryViewModel model);
        Task<Gallery> UpdateGalleryAsync(EditGalleryViewModel model);
        Task<bool> DeleteGalleryAsync(int id);
        Task<bool> ToggleGalleryStatusAsync(int id, bool isActive);

        // Access Code Management
        Task<(string clientCode, string clientPassword)> GenerateAccessCodesAsync();
        Task<(string clientCode, string clientPassword)> RegenerateAccessCodesAsync(int galleryId);
        Task<bool> ValidateAccessCodesAsync(string clientCode, string clientPassword);

        // Photo Management
        Task<bool> AddPhotosToGalleryAsync(int galleryId, List<int> photoIds);
        Task<bool> RemovePhotosFromGalleryAsync(int galleryId, List<int> photoIds);
        Task<List<PhotoSelectionViewModel>> GetAvailablePhotosAsync(int? currentGalleryId = null);

        // Session Management
        Task<IEnumerable<GallerySessionViewModel>> GetGallerySessionsAsync(int galleryId);
        Task<bool> EndSessionAsync(int sessionId);
        Task<bool> EndAllSessionsAsync(int galleryId);

        // Analytics
        Task<GalleryStatsSummaryViewModel> GetGalleryStatsAsync();
        Task<string> GetGalleryAccessUrlAsync(int galleryId, string baseUrl);
    }
}
