namespace myPhotoBiz.Blog.Models.Domain
{
    /// <summary>
    /// Represents the blog post like.
    /// </summary>
    public class BlogPostLike
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public Guid UserId { get; set; }
    }
}
