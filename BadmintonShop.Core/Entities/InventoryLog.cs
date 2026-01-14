using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BadmintonShop.Core.Entities
{
    public enum InventoryActionType
    {
        Import,     // Nhập kho
        Export,     // Xuất kho
        Adjustment  // Kiểm kê/Điều chỉnh
    }

    public class InventoryLog : BaseEntity
    {
        public int QuantityChange { get; set; }

        // [MỚI] Giá vốn tại thời điểm giao dịch (Snapshot)
        // Giúp truy xuất lịch sử giá trị kho tại thời điểm quá khứ
        public decimal CostPerUnit { get; set; }

        // [MỚI] Số lượng tồn kho ngay sau khi giao dịch thực hiện
        public int StockAfter { get; set; }

        public string Reason { get; set; }

        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public int ProductVariantId { get; set; }

        public InventoryActionType ActionType { get; set; }

        // Liên kết đơn hàng (nếu là xuất bán)
        public int? OrderId { get; set; }
        public Order Order { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}