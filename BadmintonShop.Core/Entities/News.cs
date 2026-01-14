using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Core.Entities
{
    public class News : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // Tiêu đề bài viết

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; } // URL thân thiện (seo)

        public string? ShortDescription { get; set; } // Mô tả ngắn hiện ở trang chủ

        [Required]
        public string Content { get; set; } // Nội dung bài viết (HTML)

        public string? ImageUrl { get; set; } // Ảnh đại diện bài viết
        public bool IsPublished { get; set; } = true; // Ẩn/Hiện bài viết

        // Tác giả bài viết (Admin)
        public int? AuthorId { get; set; }
        public User? Author { get; set; }
    }
}