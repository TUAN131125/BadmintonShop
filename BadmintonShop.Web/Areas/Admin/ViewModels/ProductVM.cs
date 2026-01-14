using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        public string Name { get; set; } = string.Empty;

        public string? Brand { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalePrice { get; set; }

        // Dùng List<int> để hứng nhiều Category từ Checkbox
        public List<int> CategoryIds { get; set; } = new List<int>();

        public string? Description { get; set; }

        // Đường dẫn ảnh (để hiển thị khi Edit)
        public string? ImageUrl { get; set; }

        // File ảnh upload (để hứng từ Form)
        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;
    }
}