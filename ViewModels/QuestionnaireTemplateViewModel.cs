using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// View model for creating/editing questionnaire templates with document upload.
    /// </summary>
    public class QuestionnaireTemplateViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Template Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Legacy text-based questions (optional when document is uploaded).
        /// </summary>
        [Display(Name = "Questions (Text)")]
        public string? QuestionText { get; set; }

        /// <summary>
        /// The uploaded document file (PDF or Word).
        /// </summary>
        [Display(Name = "Upload Document")]
        public IFormFile? DocumentFile { get; set; }

        /// <summary>
        /// Current document path (for display when editing).
        /// </summary>
        public string? DocumentPath { get; set; }

        /// <summary>
        /// Current original filename (for display when editing).
        /// </summary>
        public string? OriginalFileName { get; set; }

        /// <summary>
        /// Current document type.
        /// </summary>
        public string? DocumentType { get; set; }

        /// <summary>
        /// Current file size in bytes.
        /// </summary>
        public long? FileSize { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Returns true if this template has an uploaded document.
        /// </summary>
        public bool HasDocument => !string.IsNullOrEmpty(DocumentPath);

        /// <summary>
        /// Returns a human-readable file size.
        /// </summary>
        public string FileSizeDisplay
        {
            get
            {
                if (!FileSize.HasValue) return string.Empty;
                var size = FileSize.Value;
                if (size < 1024) return $"{size} B";
                if (size < 1024 * 1024) return $"{size / 1024.0:F1} KB";
                return $"{size / (1024.0 * 1024):F1} MB";
            }
        }
    }
}
