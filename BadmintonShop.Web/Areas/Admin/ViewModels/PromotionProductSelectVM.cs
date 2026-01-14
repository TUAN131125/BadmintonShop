using System.Collections.Generic;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class PromotionProductSelectVM
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;

        // Tiêu chí lọc (Filter)
        public string? Keyword { get; set; }
        public string? Brand { get; set; }
        public int? CategoryId { get; set; }

        // Danh sách kết quả tìm kiếm
        public List<ProductCheckItemVM> Products { get; set; } = new List<ProductCheckItemVM>();
    }

    public class ProductCheckItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal Price { get; set; } // Giá gốc
        public string? ImageUrl { get; set; }
        public bool IsAlreadyInPromotion { get; set; } // True nếu đã có trong sale này rồi (để disable checkbox)
    }
}