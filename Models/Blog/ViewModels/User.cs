namespace myPhotoBiz.Blog.Models.ViewModels
{
    /// <summary>
    /// Represents the user.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
    }
}
