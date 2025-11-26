
// Fixed Services/IDashboardService.cs
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FileItem>> GetFilesAsync(string filterType, int page, int pageSize);
        Task<FileItem> GetFileAsync(int id);
        Task UploadFileAsync(IFormFile file, string owner);
        Task DeleteFileAsync(int id);
    }
}