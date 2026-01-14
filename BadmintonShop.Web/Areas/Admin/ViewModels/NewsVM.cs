using System;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class NewsVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        // Dùng cho trang chi tiết
        public string Content { get; set; }
    }
}