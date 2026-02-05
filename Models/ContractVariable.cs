using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents a custom variable that can be used in contract templates.
    /// Admins can create custom variables beyond the built-in ones.
    /// </summary>
    public class ContractVariable
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Variable Name")]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9]*$", ErrorMessage = "Variable name must start with a letter and contain only letters and numbers.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Default Value")]
        public string? DefaultValue { get; set; }

        [Display(Name = "Is System Variable")]
        public bool IsSystemVariable { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the placeholder syntax for this variable (e.g., "{{VariableName}}")
        /// </summary>
        public string Placeholder => $"{{{{{Name}}}}}";
    }
}
