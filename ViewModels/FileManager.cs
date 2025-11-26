using Microsoft.AspNetCore.Identity;
using MyPhotoBiz.Models;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
    public class FileManagerViewModel
    {
        public required IEnumerable<FileItem> Files { get; set; }
        public required string FilterType { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}