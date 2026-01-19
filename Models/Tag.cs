using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPhotoBiz.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // Navigation property for the many-to-many relationship
        public required ICollection<FileItemTag> FileItemTags { get; set; }
    }
}