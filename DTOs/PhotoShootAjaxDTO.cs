using System;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.DTOs
{
    public class PhotoShootAjaxDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }
        public string? Location { get; set; }
        public PhotoShootStatus Status { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
        public int ClientId { get; set; }
        public int? PhotographerProfileId { get; set; }
    }
}
