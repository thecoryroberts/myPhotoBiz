namespace myPhotoBiz.Blog.Models.Domain
{
    /// <summary>
    /// Represents the tag.
    /// </summary>
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public ICollection<BlogPost> BlogPosts { get; set; } = null!;
    }
}
