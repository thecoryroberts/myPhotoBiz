using System;
using System.Collections.Generic;

namespace MyPhotoBiz.Models
{
    /// <summary>
    /// Print pricing configuration
    /// </summary>
    public class PrintPricing
    {
        public int Id { get; set; }
        public string? Size { get; set; }           // e.g., "4x6", "5x7", "8x10", "11x14"
        public string? FinishType { get; set; }     // e.g., "Glossy", "Matte"
        public decimal Price { get; set; }

        /// <summary>
        /// Get default print prices
        /// </summary>
        public static List<PrintPricing> GetDefaultPrices()
        {
            return new List<PrintPricing>
            {
                new PrintPricing { Id = 1, Size = "4x6", FinishType = "Glossy", Price = 0.49m },
                new PrintPricing { Id = 2, Size = "4x6", FinishType = "Matte", Price = 0.59m },
                new PrintPricing { Id = 3, Size = "5x7", FinishType = "Glossy", Price = 0.99m },
                new PrintPricing { Id = 4, Size = "5x7", FinishType = "Matte", Price = 1.09m },
                new PrintPricing { Id = 5, Size = "8x10", FinishType = "Glossy", Price = 1.99m },
                new PrintPricing { Id = 6, Size = "8x10", FinishType = "Matte", Price = 2.19m },
                new PrintPricing { Id = 7, Size = "11x14", FinishType = "Glossy", Price = 3.49m },
                new PrintPricing { Id = 8, Size = "11x14", FinishType = "Matte", Price = 3.99m },
                new PrintPricing { Id = 9, Size = "16x20", FinishType = "Glossy", Price = 8.99m },
                new PrintPricing { Id = 10, Size = "16x20", FinishType = "Matte", Price = 9.99m },
                new PrintPricing { Id = 11, Size = "16x20", FinishType = "Lustre", Price = 10.99m },
            };
        }
    }
}