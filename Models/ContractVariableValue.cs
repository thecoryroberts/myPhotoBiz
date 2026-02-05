using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Stores the actual value of a contract variable for a specific contract.
    /// This allows each contract to have its own values for custom variables.
    /// </summary>
    public class ContractVariableValue
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        public int ContractVariableId { get; set; }

        [StringLength(2000)]
        public string? Value { get; set; }

        // Navigation properties
        public virtual Contract Contract { get; set; } = null!;
        public virtual ContractVariable ContractVariable { get; set; } = null!;
    }
}
