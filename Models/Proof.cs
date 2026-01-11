// Models/Photo.cs
namespace MyPhotoBiz.Models
{

 public class Proof
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public int? GallerySessionId { get; set; }
        public string? ClientName { get; set; }          // Made nullable
        public bool IsFavorite { get; set; }
        public bool IsMarkedForEditing { get; set; }
        public string? EditingNotes { get; set; }
        public DateTime SelectedDate { get; set; }

        public virtual Photo? Photo { get; set; }
        public virtual GallerySession? Session { get; set; }
        public virtual ICollection<PrintOrder>? PrintOrders { get; set; }
    }
    
}
