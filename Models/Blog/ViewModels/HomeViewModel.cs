using myPhotoBiz.Blog.Models.Domain;

namespace myPhotoBiz.Blog.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<BlogPost> BlogPosts { get; set; } = null!;
        public IEnumerable<Tag> Tags { get; set; } = null!;
    }
}
