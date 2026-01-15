using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    public interface IFileService
    {
        // Existing methods
        Task<IEnumerable<FileItem>> GetFilesAsync(string filterType, int page, int pageSize);
        Task<FileItem?> GetFileAsync(int id);
        Task UploadFileAsync(IFormFile file, string owner);
        Task DeleteFileAsync(int id);

        // Folder methods
        Task<IEnumerable<FileItem>> GetFilesInFolderAsync(int? folderId, string filterType, int page, int pageSize);
        Task<FileItem> CreateFolderAsync(string folderName, string owner, int? parentFolderId = null);
        Task<IEnumerable<FileItem>> GetBreadcrumbsAsync(int? folderId);
        Task<bool> IsFolderEmptyAsync(int folderId);

        // Metadata methods
        Task UpdateMetadataAsync(int fileId, string? description, string? tags, bool? isFavorite);
        Task<IEnumerable<FileItem>> GetFavoritesAsync(string owner, int page, int pageSize);
        Task<IEnumerable<FileItem>> GetRecentFilesAsync(string owner, int page, int pageSize);
        Task IncrementDownloadCountAsync(int fileId);

        // Bulk upload for folders
        Task UploadFilesAsync(IFormFileCollection files, string owner, int? parentFolderId = null);

        // Client folder management
        Task<FileItem> CreateClientFolderAsync(string clientName, string owner);
        Task<FileItem?> GetClientsFolderAsync();
        Task CopyPhotoToClientFolderAsync(int clientFolderId, string sourcePath, string fileName, string owner);
    }
}