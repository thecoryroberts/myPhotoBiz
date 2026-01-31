using System;
using System.Collections.Generic;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.ViewModels
{
    public enum GalleryDownloadStatus
    {
        Success,
        Unauthorized,
        Forbidden,
        NotFound,
        InvalidRequest,
        Error
    }

    public class ClientGalleryIndexResult
    {
        public bool HasProfile { get; set; } = true;
        public List<ClientGalleryViewModel> Galleries { get; set; } = new();
    }

    public class GalleryViewPageResult
    {
        public Gallery Gallery { get; set; } = null!;
        public PaginatedList<Photo> Photos { get; set; } = null!;
        public string SessionToken { get; set; } = string.Empty;
        public int TotalPhotos { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasMorePhotos { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsPublicAccess { get; set; }
    }

    public class GalleryPhotosPageResult
    {
        public PaginatedList<Photo> Photos { get; set; } = null!;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class GallerySessionInfoResult
    {
        public Gallery Gallery { get; set; } = null!;
        public GallerySession? Session { get; set; }
    }

    public class GalleryPhotoDownloadResult
    {
        public GalleryDownloadStatus Status { get; set; } = GalleryDownloadStatus.NotFound;
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/jpeg";
    }

    public class GalleryBulkDownloadResult
    {
        public GalleryDownloadStatus Status { get; set; } = GalleryDownloadStatus.NotFound;
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public int PhotoCount { get; set; }
    }
}
