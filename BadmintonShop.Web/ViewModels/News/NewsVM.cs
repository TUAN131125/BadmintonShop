using System;

namespace BadmintonShop.Web.ViewModels.News
{
    // Dùng cho trang danh sách (Index)
    public class NewsItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Dùng cho trang chi tiết (Detail)
    public class NewsDetailVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } // Nội dung full
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Author { get; set; } = "Admin"; // Mặc định hoặc lấy từ User
    }
}