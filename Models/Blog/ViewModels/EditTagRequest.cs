namespace myPhotoBiz.Blog.Models.ViewModels
{
    /// <summary>
    /// Represents the edit tag request.
    /// </summary>
    public class EditTagRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
    }
}
