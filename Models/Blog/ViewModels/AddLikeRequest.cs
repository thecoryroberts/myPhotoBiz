namespace myPhotoBiz.Models.Blog.ViewModels
{
    /// <summary>
    /// Represents the add like request.
    /// </summary>
    public class AddLikeRequest
    {
        public Guid BlogPostId { get; set; }
        public Guid UserId { get; set; }
    }
}
