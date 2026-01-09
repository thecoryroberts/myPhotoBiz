namespace myPhotoBiz.Blog.Models.Domain
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public ICollection<BlogPost> BlogPosts { get; set; } = null!;
    }
}
