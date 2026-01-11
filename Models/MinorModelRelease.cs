using System.ComponentModel.DataAnnotations;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    public class MinorModelRelease
{
    public int id { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public Guid ClientId { get; set; }
    public string MinorName { get; set; }
    public string GuardianName { get; set; }
    public UsageRights UsageRights { get; set; }
    public string JurisdictionState { get; set; }
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