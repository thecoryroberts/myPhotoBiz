using Microsoft.AspNetCore.Identity;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels.Album;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
   public class ClientDashboardViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public List<PhotoShootViewModel> UpcomingPhotoshoots { get; set; } = new();
        public List<PhotoShootViewModel> CompletedPhotoshoots { get; set; } = new();
        public List<AlbumViewModel> AccessibleAlbums { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
    }

    public class ClientViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}