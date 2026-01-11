using MyPhotoBiz.Models;

namespace MyPhotoBiz.Models
{
    public class FileItem
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public long Size { get; set; } // in bytes
        public DateTime Modified { get; set; }
        public required string Owner { get; set; }
        public List<string> SharedWith { get; set; } = new();
        public string? FilePath { get; set; }
    }
}



// The joining entity (explicit for full control, or implicit in newer EF Core versions)
public class FileItemTag
{
    public int Id { get; set; }
    public int FileItemId { get; set; }
    public required FileItem FileItem { get; set; }

    public int TagId { get; set; }
    public required Tag Tag { get; set; }
}