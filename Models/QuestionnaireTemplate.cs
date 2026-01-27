using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents a reusable questionnaire template for pre-shoot intake.
    /// </summary>
    public class QuestionnaireTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
