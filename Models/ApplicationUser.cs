// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Defines the user type values.
    /// </summary>
    public enum UserType
    {
        Client,
        Photographer,
        Admin
    }

    /// <summary>
    /// Represents the application user.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Additional properties beyond default IdentityUser
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        [StringLength(120)]
        public string? BrandName { get; set; }
        [StringLength(240)]
        public string? BrandTagline { get; set; }
        [StringLength(7)]
        public string? BrandPrimaryColor { get; set; }
        [StringLength(7)]
        public string? BrandAccentColor { get; set; }
        [StringLength(500)]
        public string? BrandLogoLightUrl { get; set; }
        [StringLength(500)]
        public string? BrandLogoDarkUrl { get; set; }
        [StringLength(500)]
        public string? BrandCoverImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastModified { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public UserType UserType { get; set; } = UserType.Client;

        // Compatibility property: older code referenced IsPhotographer bool
        // Map this to UserType to avoid refactoring all usages immediately.
        public bool IsPhotographer
        {
            get => UserType == UserType.Photographer;
            set => UserType = value ? UserType.Photographer : UserType.Client;
        }

        // Full name property for convenience
        public string FullName => $"{FirstName} {LastName}";

        // Profile navigation properties (1:1 relationships)
        public virtual ClientProfile? ClientProfile { get; set; }
        public virtual PhotographerProfile? PhotographerProfile { get; set; }
    }
}
