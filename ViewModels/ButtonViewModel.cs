using MyPhotoBiz.Models;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{

    /// <summary>
    /// Represents view model data for a UI button, including label,
    /// command and enabled/loading state used when rendering buttons
    /// in Razor views or components.
    /// </summary>
    public class ButtonViewModel
    {
        /// <summary>Gets or sets the button type (e.g. "button", "submit", "reset").</summary>
        public string Type { get; set; } = "button"; // button, submit, reset
        /// <summary>Gets or sets the visual variant (e.g. "primary", "secondary").</summary>
        public string Variant { get; set; } = "primary"; // primary, secondary, success, danger, etc.
        /// <summary>Gets or sets the size of the button (e.g. "sm", "md", "lg").</summary>
        public string Size { get; set; } = "md"; // sm, md, lg
        /// <summary>Gets or sets an optional icon name (Tabler icon without prefix).</summary>
        public string Icon { get; set; } = string.Empty; // Tabler icon name (without ti-)
        /// <summary>Gets or sets the visible button text/label.</summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>Gets or sets whether only the icon should be shown.</summary>
        public bool IconOnly { get; set; }
        /// <summary>Gets or sets whether the button is in a loading state.</summary>
        public bool IsLoading { get; set; }
        /// <summary>Gets or sets whether the button is disabled.</summary>
        public bool IsDisabled { get; set; }
        /// <summary>Gets or sets the ARIA label for accessibility.</summary>
        public string AriaLabel { get; set; } = string.Empty;
        /// <summary>Gets or sets the client-side click handler or JS command.</summary>
        public string OnClick { get; set; } = string.Empty;
        /// <summary>Gets or sets additional CSS classes to apply to the button.</summary>
        public string AdditionalClasses { get; set; } = string.Empty;   
    }
}