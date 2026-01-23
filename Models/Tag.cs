using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents the tag.
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // Navigation property for the many-to-many relationship
        public ICollection<FileItemTag> FileItemTags { get; set; } = new List<FileItemTag>();
    }
}
