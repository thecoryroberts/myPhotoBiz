namespace myPhotoBiz.Blog.Models.ViewModels
{
    /// <summary>
    /// Represents view model data for user.
    /// </summary>
    public class UserViewModel
    {
        public List<User> Users { get; set; } = null!;

        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool AdminRoleCheckbox { get; set; }
    }
}
