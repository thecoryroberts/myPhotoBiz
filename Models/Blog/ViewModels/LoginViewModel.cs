using System.ComponentModel.DataAnnotations;

namespace myPhotoBiz.Blog.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Password has to be at least 6 characters")]
        public string Password { get; set; } = null!;
        public string? ReturnUrl { get; set; }
    }
}
