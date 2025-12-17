namespace myPhotoBiz.Blog.Models.Domain
{
    public class BlogPostComment
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = null!;
        public Guid BlogPostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
