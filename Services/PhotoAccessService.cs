using System.Security.Claims;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for handling photo access authorization logic
    /// Centralizes the repeated authorization patterns from PhotosController
    /// </summary>
    public class PhotoAccessService : IPhotoAccessService
    {
        private readonly IClientService _clientService;

        public PhotoAccessService(IClientService clientService)
        {
            _clientService = clientService;
        }

        /// <summary>
        /// Checks if the current user can access a photo
        /// </summary>
        public async Task<PhotoAccessResult> CanAccessPhotoAsync(Photo photo, ClaimsPrincipal user)
        {
            // Check if photo is in a public gallery (accessible to anyone)
            if (IsPhotoInPublicGallery(photo))
            {
                return PhotoAccessResult.Allowed();
            }

            // Not authenticated and not public
            if (user.Identity?.IsAuthenticated != true)
            {
                return PhotoAccessResult.Denied("Authentication required");
            }

            // Staff can access all photos
            if (IsStaffUser(user))
            {
                return PhotoAccessResult.Allowed();
            }

            // For clients, verify they own the photo
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return PhotoAccessResult.Denied("User not found");
            }

            var client = await _clientService.GetClientByUserIdAsync(userId);
            if (client == null)
            {
                return PhotoAccessResult.Denied("Client profile not found");
            }

            // Check if the photo belongs to this client's photoshoot
            if (photo.Album?.PhotoShoot?.ClientProfileId != client.Id)
            {
                return PhotoAccessResult.Denied("Access denied to this photo");
            }

            return PhotoAccessResult.Allowed();
        }

        /// <summary>
        /// Checks if a photo is in a public (active, non-expired) gallery
        /// </summary>
        public bool IsPhotoInPublicGallery(Photo photo)
        {
            return photo.Album?.Galleries?.Any(g => g.IsActive && g.ExpiryDate > DateTime.UtcNow) ?? false;
        }

        private static bool IsStaffUser(ClaimsPrincipal user)
        {
            return user.IsInRole(AppConstants.Roles.Admin) ||
                   user.IsInRole(AppConstants.Roles.Photographer);
        }
    }
}
