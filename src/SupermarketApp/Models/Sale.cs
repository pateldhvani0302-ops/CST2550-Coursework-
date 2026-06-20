using System;

namespace SupermarketApp.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Populated in-memory after a sale is recorded. Not mapped by EF directly
        /// (see SupermarketContext.OnModelCreating) - line items are persisted via SaleItem.
        /// </summary>
        public SaleItem[]? Items { get; set; }
    }
}
