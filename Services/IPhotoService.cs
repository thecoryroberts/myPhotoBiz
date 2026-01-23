
// Services/IPhotoService.cs
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the photo service contract.
    /// </summary>
    public interface IPhotoService
    {
        Task<IEnumerable<Photo>> GetPhotosByAlbumIdAsync(int albumId);
        Task<Photo?> GetPhotoByIdAsync(int id);
        Task<IEnumerable<Photo>> GetPhotosByClientIdAsync(int clientId);
        Task<Photo> CreatePhotoAsync(Photo photo);
        Task<bool> DeletePhotoAsync(int id);
        Task<Photo> UpdatePhotoAsync(Photo photo);
        Task<string> SavePhotoAsync(IFormFile file, string uploadsPath);
    }
}
