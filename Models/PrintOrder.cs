// Models/PrintOrder.cs
using System;
using System.Collections.Generic;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Represents a customer print order
    /// </summary>
    public class PrintOrder
    {
        public int Id { get; set; }
        public int GallerySessionId { get; set; }
        public string? OrderNumber { get; set; }
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? PrintLabOrderId { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime? RefundedDate { get; set; }

        // Navigation properties
        public virtual GallerySession? Session { get; set; }
        public virtual ICollection<PrintItem> Items { get; set; } = new List<PrintItem>();
    }
}