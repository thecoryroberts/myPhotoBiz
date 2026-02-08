using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
    public class QuestionnaireResponseViewModel
    {
        [Required]
        public int AssignmentId { get; set; }

        public string QuestionnaireName { get; set; } = string.Empty;

        public string QuestionText { get; set; } = string.Empty;

        public List<QuestionnaireResponseItemViewModel> Items { get; set; } = new();

        public string? LegacyResponseText { get; set; }

        public bool IsCompleted { get; set; }
    }

    public class QuestionnaireResponseItemViewModel
    {
        public string Section { get; set; } = string.Empty;

        public string Question { get; set; } = string.Empty;

        public string Type { get; set; } = "text";

        public List<string> Options { get; set; } = new();

        public string Answer { get; set; } = string.Empty;

        public bool? AnswerBool { get; set; }

        public List<string> AnswerList { get; set; } = new();
    }
}
