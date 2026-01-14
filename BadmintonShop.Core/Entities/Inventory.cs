using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Entities
{
    public class Inventory : BaseEntity
    {
        public int Quantity { get; set; }

        // [MỚI] Giá vốn bình quân (Moving Average Cost)
        // Dùng decimal để đảm bảo độ chính xác tiền tệ
        public decimal AverageCost { get; set; } = 0;

        // [MỚI] Mức tồn kho tối thiểu (Cảnh báo)
        // Nếu null: Sử dụng cấu hình chung của kho (Global Config)
        // Nếu có giá trị: Sử dụng cấu hình riêng của sản phẩm này
        public int? MinStock { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public ICollection<InventoryLog> Logs { get; set; }
    }
}