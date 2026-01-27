using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Tracks assignment of a questionnaire template to a user.
    /// </summary>
    public class QuestionnaireAssignment
    {
        public int Id { get; set; }

        [Required]
        public int QuestionnaireTemplateId { get; set; }

        public QuestionnaireTemplate? QuestionnaireTemplate { get; set; }

        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;

        public ApplicationUser? AssignedToUser { get; set; }

        [Required]
        public string AssignedByUserId { get; set; } = string.Empty;

        public ApplicationUser? AssignedByUser { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public QuestionnaireAssignmentStatus Status { get; set; } = QuestionnaireAssignmentStatus.Assigned;
    }

    public enum QuestionnaireAssignmentStatus
    {
        Assigned = 0,
        Completed = 1
    }
}
