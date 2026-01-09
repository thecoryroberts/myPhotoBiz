using System.ComponentModel.DataAnnotations;

namespace myPhotoBiz.Blog.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Password has to be at least 6 characters")]
        public string Password { get; set; } = null!;
    }
}
