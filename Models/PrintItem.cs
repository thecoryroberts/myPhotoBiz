// Models/PrintItem.cs
using System;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents an individual print item in an order
    /// </summary>
    public class PrintItem
    {
        public int Id { get; set; }
        public int PrintOrderId { get; set; }
        public int PhotoId { get; set; }
        public string? Size { get; set; }           // e.g., "4x6", "5x7", "8x10"
        public int Quantity { get; set; }
        public string? FinishType { get; set; }     // e.g., "Glossy", "Matte"
        public decimal UnitPrice { get; set; }

        // Navigation properties
        public virtual PrintOrder? PrintOrder { get; set; }
        public virtual Photo? Photo { get; set; }
    }
}