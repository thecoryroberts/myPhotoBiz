using myPhotoBiz.Blog.Models.Domain;

namespace myPhotoBiz.Blog.Models.ViewModels
{
    public class BlogDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Heading { get; set; } = null!;
        public string PageTitle { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ShortDescription { get; set; } = null!;
        public string FeaturedImageUrl { get; set; } = null!;
        public string UrlHandle { get; set; } = null!;
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; } = null!;
        public bool Visible { get; set; }
        public ICollection<Tag> Tags { get; set; } = null!;

        public int TotalLikes { get; set; }

        public bool Liked { get; set; }
        public string CommentDescription { get; set; } = null!;

        public IEnumerable<BlogComment> Comments { get; set; } = null!;
    }
}
