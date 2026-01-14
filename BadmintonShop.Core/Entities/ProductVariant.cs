using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Entities
{
    public class ProductVariant : BaseEntity
    {
        public string SKU { get; set; }

        public string AttributeName { get; set; }   // VD: Weight, Grip
        public string AttributeValue { get; set; }  // VD: 4U, G5

        // Giá gốc của biến thể (Ví dụ: Vợt 3U giá 1tr, 4U giá 1tr2)
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public Inventory Inventory { get; set; }

        // =========================================================
        // [NOT MAPPED] DÙNG CHO GIỎ HÀNG & THANH TOÁN
        // =========================================================

        [NotMapped]
        public decimal CurrentPrice { get; set; } // Giá bán thực tế của Variant này (Sau khi áp dụng Promotion của Product cha)

        [NotMapped]
        public bool IsOnSale { get; set; }

        [NotMapped]
        public decimal DiscountAmount { get; set; }
    }
}