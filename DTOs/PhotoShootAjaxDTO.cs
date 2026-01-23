
// ======================================================================
// 📌 DTO for AJAX Calendar Modal (clean and minimal)
// ======================================================================

using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.DTOs
{
    /// <summary>
    /// Represents the photo shoot ajax dto.
    /// </summary>
    public class PhotoShootAjaxDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }

        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Notes { get; set; }
        public PhotoShootStatus Status { get; set; }
        public int ClientId { get; set; }
        public int? PhotographerProfileId { get; set; }
    }
}
