namespace myPhotoBiz.Blog.Models.Domain
{
    /// <summary>
    /// Represents the blog post comment.
    /// </summary>
    public class BlogPostComment
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = null!;
        public Guid BlogPostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
