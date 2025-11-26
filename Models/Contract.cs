using MyPhotoBiz.Enums;
namespace MyPhotoBiz.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; } // HTML or Markdown
        public required string ClientName { get; set; }
        public required DateTime CreatedDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public required string SignatureImagePath { get; set; }
        public ContractStatus Status { get; set; }
    }
}