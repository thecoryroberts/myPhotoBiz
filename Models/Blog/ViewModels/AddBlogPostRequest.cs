using Microsoft.AspNetCore.Mvc.Rendering;

namespace myPhotoBiz.Models.Blog.ViewModels
{
    /// <summary>
    /// Represents the add blog post request.
    /// </summary>
    public class AddBlogPostRequest
    {
        public string Heading { get; set; } = null!;
        public string PageTitle { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ShortDescription { get; set; } = null!;
        public string FeaturedImageUrl { get; set; } = null!;
        public string UrlHandle { get; set; } = null!;
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; } = null!;
        public bool Visible { get; set; }


        // Display Tags
        public IEnumerable<SelectListItem> Tags { get; set; } = null!;
        // Collect Tags
        public string[] SelectedTags { get; set; } = Array.Empty<string>();
    }
}
