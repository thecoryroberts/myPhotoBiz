using Microsoft.AspNetCore.Html;
using System;

namespace MyPhotoBiz.ViewModels
{
    public class IndexTableShellViewModel
    {
        public bool HasItems { get; set; }
        public Func<object?, IHtmlContent>? Thead { get; set; }
        public Func<object?, IHtmlContent>? Tbody { get; set; }
        public Func<object?, IHtmlContent>? EmptyContent { get; set; }
        public string TableClass { get; set; } = "table table-custom table-centered table-select table-hover w-100 mb-0";
        public string? EmptyIcon { get; set; }
        public string EmptyTitle { get; set; } = "No Records Found";
        public string? EmptyMessage { get; set; }
    }
}
