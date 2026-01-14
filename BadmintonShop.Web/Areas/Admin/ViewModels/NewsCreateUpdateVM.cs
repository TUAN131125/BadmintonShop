using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.ViewModels.News
{
    public class NewsCreateUpdateVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public bool IsPublished { get; set; } = true;

        // Xử lý upload ảnh
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? ImageFile { get; set; }
    }
}