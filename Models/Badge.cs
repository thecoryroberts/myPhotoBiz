using System.ComponentModel.DataAnnotations;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    public class Badge
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public BadgeType Type { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; } // Font icon class or emoji

        [StringLength(20)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code like #ffffff")]
        public string Color { get; set; } = "#6c757d"; // Badge color (hex)

        public bool IsActive { get; set; } = true;

        // Auto-award configuration
        public bool AutoAward { get; set; } = false;
        public int? RequiredContractId { get; set; } // Auto-award when this contract is signed

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<ClientBadge> ClientBadges { get; set; } = new List<ClientBadge>();
    }
}
