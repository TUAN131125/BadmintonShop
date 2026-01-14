using BadmintonShop.Core.Entities;
using BadmintonShop.Web.ViewModels.News;

namespace BadmintonShop.Web.ViewModels.Store
{
    public class HomeStoreViewModel
    {
        // 1. Banner
        public List<Banner> Banners { get; set; } = new();

        // 2. Danh mục nổi bật (Categories)
        public List<Category> FeaturedCategories { get; set; } = new();

        // 3. Các nhóm sản phẩm
        public List<ProductItemViewModel> FeaturedProducts { get; set; } = new(); // Nổi bật
        public List<ProductItemViewModel> NewArrivals { get; set; } = new();      // Mới về
        public List<ProductItemViewModel> OnSaleProducts { get; set; } = new();   // Đang giảm giá
        public List<NewsItemVM> LatestNews { get; set; } = new();
    }
}