// Models/Photo.cs
namespace MyPhotoBiz.Models
{
       public class GallerySession
    {
        public int Id { get; set; }
        public int GalleryId { get; set; }
        public required string SessionToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessDate { get; set; }
        public ICollection<Proof>? Proofs { get; set; }
        public required Gallery Gallery { get; set; }
    }
}