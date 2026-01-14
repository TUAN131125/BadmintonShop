using System.Collections.Generic;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class ProductDetailAdminVM
    {
        // --- Thông tin chung ---
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }

        // --- Thống kê Kho (Inventory Stats) ---
        public int TotalStock { get; set; }         // Tổng tồn kho
        public int TotalVariants { get; set; }      // Tổng số biến thể (size/màu)
        public decimal TotalStockValue { get; set; } // Tổng giá trị vốn (Quantity * Cost)

        // --- Danh sách biến thể ---
        public List<VariantDetailVM> Variants { get; set; } = new List<VariantDetailVM>();
    }

    public class VariantDetailVM
    {
        public string SKU { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        public decimal ImportPrice { get; set; } // Giá vốn bình quân
        public decimal SalePrice { get; set; }   // Giá bán
        public int Stock { get; set; }

        // Lợi nhuận gộp dự kiến (1 sản phẩm)
        public decimal EstimatedProfit => SalePrice - ImportPrice;
    }
}