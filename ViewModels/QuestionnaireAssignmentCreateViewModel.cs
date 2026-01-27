using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// View model for assigning questionnaire templates to users.
    /// </summary>
    public class QuestionnaireAssignmentCreateViewModel
    {
        [Required]
        public int QuestionnaireTemplateId { get; set; }

        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public List<SelectListItem> TemplateOptions { get; set; } = new();

        public List<SelectListItem> UserOptions { get; set; } = new();
    }
}
