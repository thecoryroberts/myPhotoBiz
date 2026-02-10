namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Join entity linking a ServicePackage to a ContractTemplate.
    /// When a booking with this package is converted to a photo shoot,
    /// the linked contract templates are automatically created and sent.
    /// </summary>
    public class ServicePackageContractTemplate
    {
        public int Id { get; set; }

        public int ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; } = null!;

        public int ContractTemplateId { get; set; }
        public ContractTemplate ContractTemplate { get; set; } = null!;
    }
}
