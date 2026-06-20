using System;

namespace SupermarketApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        public DateTime ExpiryOrRestockDate { get; set; }

        public StockStatus GetStockStatus()
        {
            if (QuantityInStock <= 0) return StockStatus.OutOfStock;
            if (QuantityInStock <= LowStockThreshold) return StockStatus.LowStock;
            return StockStatus.InStock;
        }

        public override string ToString()
        {
            return $"[{ProductId}] {Title} | Barcode: {Barcode} | Price: {Price:C} | Qty: {QuantityInStock} | Status: {GetStockStatus()}";
        }
    }
}
