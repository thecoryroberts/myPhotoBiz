namespace myPhotoBiz.Blog.Models.ViewModels
{
    /// <summary>
    /// Represents the blog comment.
    /// </summary>
    public class BlogComment
    {
        public string Description { get; set; } = null!;
        public DateTime DateAdded { get; set; }
        public string Username { get; set; } = null!;
    }
}
