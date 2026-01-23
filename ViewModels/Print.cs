using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Represents view model data for print item.
    /// </summary>
    public class PrintItemViewModel
    {
        public int PhotoId { get; set; }
        public string? Size { get; set; }
        public string? FinishType { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Represents view model data for print order.
    /// </summary>
    public class PrintOrderViewModel
    {
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public List<PrintItemViewModel> Items { get; set; } = new();
    }
}
