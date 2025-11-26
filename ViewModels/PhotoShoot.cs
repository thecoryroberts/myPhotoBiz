using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyPhotoBiz.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
   
    public class PhotoShootViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        // Dropdown list for selecting client in the view
        public IEnumerable<SelectListItem>? Clients { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [Display(Name = "Updated Date")]
        public DateTime UpdatedDate { get; set; }
        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        public PhotoShootStatus Status { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string? Notes { get; set; }

        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }
        // Optional Client details (not required for create/edit, but useful on dashboards)
        public MyPhotoBiz.Models.Client? Client { get; set; }
    }


}