using System.Security.Claims;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for handling photo access authorization logic
    /// </summary>
    public interface IPhotoAccessService
    {
        /// <summary>
        /// Checks if the current user can access a photo
        /// </summary>
        Task<PhotoAccessResult> CanAccessPhotoAsync(Photo photo, ClaimsPrincipal user);

        /// <summary>
        /// Checks if a photo is in a public (active, non-expired) gallery
        /// </summary>
        bool IsPhotoInPublicGallery(Photo photo);
    }

    /// <summary>
    /// Result of photo access check
    /// </summary>
    public class PhotoAccessResult
    {
        public bool IsAllowed { get; set; }
        public string? DenialReason { get; set; }

        public static PhotoAccessResult Allowed() => new() { IsAllowed = true };
        public static PhotoAccessResult Denied(string reason) => new() { IsAllowed = false, DenialReason = reason };
    }
}
