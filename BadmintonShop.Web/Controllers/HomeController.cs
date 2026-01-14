using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.ViewModels.Store;
using BadmintonShop.Web.ViewModels.News;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IBannerService _bannerService;
        private readonly ICategoryService _categoryService;
        private readonly INewsService _newsService;

        public HomeController(
            IProductService productService,
            IBannerService bannerService,
            ICategoryService categoryService,
            INewsService newsService)
        {
            _productService = productService;
            _bannerService = bannerService;
            _categoryService = categoryService;
            _newsService = newsService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. TRUY V?N D? LI?U
            var banners = await _bannerService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();

            // L?y 8 s?n ph?m cho m?i m?c
            var featuredProducts = await _productService.GetFeaturedProductsAsync(8);
            var newArrivals = await _productService.GetNewArrivalsAsync(8);
            var onSaleProducts = await _productService.GetOnSaleProductsAsync(8);

            // [S?A L?I 1]: Dùng GetPublishedAsync() r?i l?y 3 bài, thay vì g?i hàm không t?n t?i
            var allNews = await _newsService.GetPublishedAsync();
            var latestNews = allNews.Take(3);

            // 2. MAP D? LI?U SANG VIEWMODEL
            var vm = new HomeStoreViewModel
            {
                // Banner: Ch? l?y cái ?ang Active và s?p x?p
                Banners = banners
                            .Where(x => x.IsActive)
                            .OrderBy(x => x.OrderIndex)
                            .ToList(),

                // Category: L?y danh m?c g?c (ParentId == null)
                FeaturedCategories = categories
                                        .Where(c => c.ParentId == null)
                                        .Take(4)
                                        .ToList(),

                // Map Entity sang ProductItemViewModel
                FeaturedProducts = MapToProductVM(featuredProducts),
                NewArrivals = MapToProductVM(newArrivals),
                OnSaleProducts = MapToProductVM(onSaleProducts),

                // [S?A L?I 2]: Dùng NewsItemVM (kh?p v?i file NewsVM.cs)
                LatestNews = latestNews.Select(n => new NewsItemVM
                {
                    Id = n.Id,
                    Title = n.Title,
                    ImageUrl = n.ImageUrl,
                    ShortDescription = n.ShortDescription,
                    CreatedAt = n.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        // Helper: Chuy?n ??i t? Entity Product sang ProductItemViewModel
        private List<ProductItemViewModel> MapToProductVM(IEnumerable<BadmintonShop.Core.Entities.Product> products)
        {
            return products.Select(p => new ProductItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ImageUrl = p.ImageUrl,
                Brand = p.Brand,

                BasePrice = p.BasePrice,

                // [QUAN TR?NG] Map d? li?u ?ã tính toán t? ProductService
                CurrentPrice = p.CurrentPrice,
                IsOnSale = p.IsOnSale,

                // Tính % gi?m giá ?? hi?n th? Badge
                DiscountPercent = (p.IsOnSale && p.BasePrice > 0)
                    ? (int)Math.Round((1 - (p.CurrentPrice / p.BasePrice)) * 100)
                    : 0,

                // Fallback: N?u không có logic m?i thì dùng logic c?
                SalePrice = p.SalePrice
            }).ToList();
        }
    }
}