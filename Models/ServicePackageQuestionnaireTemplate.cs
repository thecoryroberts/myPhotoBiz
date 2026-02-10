namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Join entity linking a ServicePackage to a QuestionnaireTemplate.
    /// When a booking with this package is converted to a photo shoot,
    /// the linked questionnaire templates are automatically assigned to the client.
    /// </summary>
    public class ServicePackageQuestionnaireTemplate
    {
        public int Id { get; set; }

        public int ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; } = null!;

        public int QuestionnaireTemplateId { get; set; }
        public QuestionnaireTemplate QuestionnaireTemplate { get; set; } = null!;
    }
}
