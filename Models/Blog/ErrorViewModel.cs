namespace myPhotoBiz.Blog.Models
{
    /// <summary>
    /// Represents view model data for error.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
