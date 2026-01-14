using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }

        public string Brand { get; set; }
        public string Description { get; set; }

        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }

        // =========================================================
        // [NOT MAPPED] CÁC TRƯỜNG TÍNH TOÁN (KHÔNG LƯU DB)
        // =========================================================

        [NotMapped]
        public decimal CurrentPrice { get; set; } // Giá thực tế (đã trừ KM)

        [NotMapped]
        public bool IsOnSale { get; set; } // Cờ báo đang có giảm giá

        [NotMapped]
        public decimal DiscountAmount { get; set; } // Số tiền được giảm (để hiện: Tiết kiệm 50k)

        [NotMapped]
        public Promotion? ActivePromotion { get; set; } // Thông tin đợt KM (nếu có)

        // =========================================================

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<ProductVariant> Variants { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}