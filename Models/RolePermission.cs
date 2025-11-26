using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Models
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        public string Permission { get; set; } = string.Empty;
    }
}
