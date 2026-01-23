using System.ComponentModel.DataAnnotations;

namespace myPhotoBiz.Models.Blog.ViewModels
{
    /// <summary>
    /// Represents the add tag request.
    /// </summary>
    public class AddTagRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string DisplayName { get; set; } = null!;
    }
}
