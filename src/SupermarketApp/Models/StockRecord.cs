using System;

namespace SupermarketApp.Models
{
    public enum StockMovementType
    {
        Restock,
        Sale,
        Adjustment
    }

    public class StockRecord
    {
        public int StockRecordId { get; set; }
        public int ProductId { get; set; }
        public StockMovementType MovementType { get; set; }

        /// <summary>Positive for restocks/positive adjustments, negative for sales/negative adjustments.</summary>
        public int QuantityChange { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }
}
