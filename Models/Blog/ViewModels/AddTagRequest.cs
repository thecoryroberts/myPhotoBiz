using System.ComponentModel.DataAnnotations;

namespace myPhotoBiz.Models.Blog.ViewModels
{
    public class AddTagRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string DisplayName { get; set; } = null!;
    }
}
