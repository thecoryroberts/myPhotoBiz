namespace myPhotoBiz.Models.Blog.ViewModels
{
    public class AddLikeRequest
    {
        public Guid BlogPostId { get; set; }
        public Guid UserId { get; set; }
    }
}
