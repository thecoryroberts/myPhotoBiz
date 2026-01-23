using System;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents the contract template.
    /// </summary>
    public class ContractTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Template content with placeholders like {{ClientName}}, {{ClientEmail}}, {{PhotoShootDate}}, etc.
        /// </summary>
        public string? ContentTemplate { get; set; }

        /// <summary>
        /// Optional PDF template file path
        /// </summary>
        [StringLength(500)]
        public string? PdfFilePathTemplate { get; set; }

        /// <summary>
        /// Category for organizing templates (e.g., "Wedding", "Portrait", "Commercial")
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Whether this template is active and should be shown in selection dropdowns
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When this template was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether to award a badge when contract from this template is signed
        /// </summary>
        public bool AwardBadgeOnSign { get; set; } = false;

        /// <summary>
        /// Badge to award if AwardBadgeOnSign is true
        /// </summary>
        public int? BadgeToAwardId { get; set; }

        public Badge? BadgeToAward { get; set; }
    }
}
