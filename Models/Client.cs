
// Models/Client.cs
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public PreferredContactMethod Method { get; set; } = PreferredContactMethod.Email;

        // Foreign keys
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Navigation properties
        public virtual ICollection<PhotoShoot> PhotoShoots { get; set; } = new List<PhotoShoot>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<ClientBadge> ClientBadges { get; set; } = new List<ClientBadge>();
    }

    public enum PreferredContactMethod
    {
        Email = 0,
        Text = 1,
        Call = 2,
    }
}