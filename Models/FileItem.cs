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


