using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class InventoryActionVM
    {
        public int ProductVariantId { get; set; }
        public string VariantInfo { get; set; } // Hiển thị tên/SKU để admin biết đang sửa cái gì

        [Required]
        public int Quantity { get; set; } // Cho phép số âm (xuất) hoặc dương (nhập)

        public string Note { get; set; }
    }
}