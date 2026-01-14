using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.ViewModels.Store;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductVariantService _variantService;

        public ProductController(
            IProductService productService,
            IProductVariantService variantService)
        {
            _productService = productService;
            _variantService = variantService;
        }

        // TRANG DANH SÁCH (Dành cho khách hàng)
        public async Task<IActionResult> Index(
            string q,
            List<string>? brands,
            List<string>? prices,
            string? sort,
            int? categoryId)
        {
            // 1. Lấy tất cả sản phẩm Active (Service đã tự động tính CurrentPrice và IsOnSale)
            var data = await _productService.GetAllAsync(includeInactive: false);

            // 2. Lọc theo Danh mục
            if (categoryId.HasValue)
            {
                data = data.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));
            }

            // 3. Lọc theo Thương hiệu
            if (brands?.Any() == true)
            {
                data = data.Where(p => brands.Contains(p.Brand));
            }

            // 4. Lọc theo Giá (Lưu ý: Logic lọc này đang dựa trên BasePrice, nếu muốn chuẩn user thì nên sửa thành CurrentPrice)
            if (prices?.Any() == true)
            {
                var filtered = new List<Product>();
                if (prices.Contains("under15")) filtered.AddRange(data.Where(p => p.BasePrice < 1500000));
                if (prices.Contains("15-25")) filtered.AddRange(data.Where(p => p.BasePrice >= 1500000 && p.BasePrice <= 2500000));
                if (prices.Contains("over25")) filtered.AddRange(data.Where(p => p.BasePrice > 2500000));
                data = filtered.Distinct();
            }

            // 5. Sắp xếp
            switch (sort)
            {
                case "price_asc":
                    // Sắp xếp theo giá thực tế phải trả
                    data = data.OrderBy(p => p.CurrentPrice);
                    break;
                case "price_desc":
                    data = data.OrderByDescending(p => p.CurrentPrice);
                    break;
                default:
                    data = data.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // Logic tìm kiếm
            if (!string.IsNullOrEmpty(q))
            {
                q = q.ToLower().Trim();
                data = data.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(q)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(q)) ||
                    (p.Description != null && p.Description.ToLower().Contains(q)) ||
                    (p.Variants != null && p.Variants.Any(v => v.SKU != null && v.SKU.ToLower().Contains(q))) ||
                    (p.Categories != null && p.Categories.Any(c => c.Name != null && c.Name.ToLower().Contains(q)))
                );
            }

            // 6. Map ra ViewModel (CẬP NHẬT LOGIC MỚI)
            var vm = new ProductListViewModel
            {
                Brands = brands,
                Prices = prices,
                Products = data.Select(p => new ProductItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    ImageUrl = p.ImageUrl,
                    Brand = p.Brand,

                    // [MỚI] Map đúng dữ liệu đã tính toán từ Service
                    BasePrice = p.BasePrice,
                    CurrentPrice = p.CurrentPrice, // Giá đã giảm (Promotion hoặc SalePrice)
                    IsOnSale = p.IsOnSale,

                    // Tính % để hiện Badge
                    DiscountPercent = p.IsOnSale && p.BasePrice > 0
                        ? (int)Math.Round((1 - (p.CurrentPrice / p.BasePrice)) * 100)
                        : 0,

                    // Giữ lại DisplayPrice để fallback nếu View cũ còn dùng, nhưng gán bằng CurrentPrice
                    DisplayPrice = p.CurrentPrice
                }).ToList()
            };

            // Lưu trạng thái lọc để View sử dụng lại
            ViewBag.CurrentSearch = q;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentCategoryId = categoryId;

            return View(vm);
        }

        // TRANG CHI TIẾT (Dành cho khách hàng)
        public async Task<IActionResult> Detail(int id)
        {
            // 1. Lấy sản phẩm (Service đã tính giá cho cả Product và Variants bên trong)
            var p = await _productService.GetByIdAsync(id);
            if (p == null) return NotFound();

            // [QUAN TRỌNG] KHÔNG gọi _variantService.GetByProductIdAsync(id) nữa
            // Dùng trực tiếp p.Variants đã được Service xử lý logic giá.

            // 2. XỬ LÝ SẢN PHẨM LIÊN QUAN
            var allProducts = await _productService.GetAllAsync(includeInactive: false);

            var relatedProducts = allProducts
                .Where(x => x.Categories.Any(c => p.Categories.Any(pc => pc.Id == c.Id)) && x.Id != id)
                .Take(4)
                .Select(x => new ProductItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Brand = x.Brand,
                    ImageUrl = x.ImageUrl,

                    // Map giá mới cho sản phẩm liên quan
                    BasePrice = x.BasePrice,
                    CurrentPrice = x.CurrentPrice,
                    IsOnSale = x.IsOnSale,
                    DiscountPercent = x.IsOnSale && x.BasePrice > 0
                        ? (int)Math.Round((1 - (x.CurrentPrice / x.BasePrice)) * 100)
                        : 0
                }).ToList();

            // 3. Map ViewModel chi tiết (CẬP NHẬT LOGIC MỚI)
            var vm = new ProductDetailViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Brand = p.Brand,
                ImageUrl = p.ImageUrl,
                Description = p.Description,

                // Giá hiển thị
                BasePrice = p.BasePrice,
                CurrentPrice = p.CurrentPrice,
                IsOnSale = p.IsOnSale,
                DiscountAmount = p.DiscountAmount, // Số tiền được giảm

                // Hiển thị tên danh mục
                CategoryName = (p.Categories != null && p.Categories.Any())
                                ? string.Join(", ", p.Categories.Select(c => c.Name))
                                : "Uncategorized",

                // Map Variants (Lấy từ p.Variants thay vì query lại)
                Variants = (p.Variants ?? new List<ProductVariant>()).Select(v => new ViewModels.Store.ProductVariantVM
                {
                    Id = v.Id,
                    AttributeName = v.AttributeName,
                    AttributeValue = v.AttributeValue,

                    // [QUAN TRỌNG] Map giá gốc và giá sau giảm của từng Variant
                    OriginalPrice = v.Price,       // Giá gốc của biến thể
                    CurrentPrice = v.CurrentPrice, // Giá thực tế bán ra

                    Stock = v.Inventory != null ? v.Inventory.Quantity : 0
                }).OrderBy(v => v.CurrentPrice).ToList(), // Sắp xếp giá rẻ nhất lên đầu

                RelatedProducts = relatedProducts
            };

            return View(vm);
        }

        // Action live search
        [HttpGet]
        [Route("api/products/search-suggestions")]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return Ok(new List<object>());

            var allProducts = await _productService.GetAllAsync(includeInactive: false);

            query = query.ToLower().Trim();

            var suggestions = allProducts
                .Where(p => p.Name.ToLower().Contains(query) || p.Brand.ToLower().Contains(query))
                .Take(5)
                .Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    image = p.ImageUrl,
                    // Cập nhật dùng CurrentPrice cho nhất quán
                    price = p.CurrentPrice,
                    url = Url.Action("Detail", "Product", new { id = p.Id })
                });

            return Ok(suggestions);
        }
    }
}