using System.ComponentModel.DataAnnotations;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents the contracts.
    /// </summary>
    public class Contracts
    {
        public int ContractId { get; set; }
        public ContractType ContractType { get; set; }
        public DateTime SessionDate { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal RetainerAmount { get; set; }
        public UsageRights UsageRights { get; set; }
        public string DeliveryTimeline { get; set; } = string.Empty;
        public DateTime AcceptedAt { get; set; }
        public AcceptanceMethod AcceptanceMethod { get; set; }
        public int ContractVersion { get; set; }
        public bool IsArchived { get; set; }


        // Foreign keys - Client relationship via ClientProfile
        [Required]
        public int ClientProfileId { get; set; }
        public virtual ClientProfile ClientProfile { get; set; } = null!;
    }
}
