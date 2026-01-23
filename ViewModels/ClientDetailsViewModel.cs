using MyPhotoBiz.Models;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Represents view model data for client details.
    /// </summary>
    public class ClientDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Include the linked ApplicationUser for profile picture info
        public ApplicationUser? User { get; set; }

        // Summary numbers
        public int PhotoShootCount { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<PhotoShootViewModel> PhotoShoots { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
        public List<ClientBadge> ClientBadges { get; set; } = new();
        public List<Contract> Contracts { get; set; } = new();
    }
}
