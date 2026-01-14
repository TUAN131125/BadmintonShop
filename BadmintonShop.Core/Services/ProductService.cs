using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Helpers;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ==================================================================================
        // 1. CORE PRICING ENGINE (BỘ XỬ LÝ GIÁ TRUNG TÂM - QUAN TRỌNG NHẤT)
        // ==================================================================================
        public async Task ApplyPromotionLogic(IEnumerable<Product> products)
        {
            var now = DateTime.Now;

            // Lấy tất cả Promotion đang chạy (Active & Còn hạn)
            // Cần Include PromotionProducts để biết SP nào được giảm
            var activePromotions = await _unitOfWork.PromotionRepository.GetQuery()
                .Include(p => p.PromotionProducts)
                .Where(p => p.IsActive && !p.IsDeleted && p.StartDate <= now && p.EndDate >= now)
                .ToListAsync();

            foreach (var p in products)
            {
                // --- A. TÌM KHUYẾN MÃI TỐT NHẤT CHO SẢN PHẨM ---
                Promotion? bestPromo = activePromotions
                    .Where(promo => promo.PromotionProducts.Any(pp => pp.ProductId == p.Id))
                    .OrderByDescending(promo => promo.IsPercent
                        ? p.BasePrice * (promo.DiscountValue / 100)
                        : promo.DiscountValue)
                    .FirstOrDefault();

                p.ActivePromotion = bestPromo;

                // --- B. TÍNH GIÁ CHO BIẾN THỂ (VARIANTS) ---
                if (p.Variants != null && p.Variants.Any())
                {
                    foreach (var v in p.Variants)
                    {
                        decimal finalVariantPrice = v.Price;
                        bool variantHasSale = false;

                        if (bestPromo != null)
                        {
                            decimal discountAmt = bestPromo.IsPercent
                                ? v.Price * (bestPromo.DiscountValue / 100)
                                : bestPromo.DiscountValue;

                            decimal promoPrice = v.Price - discountAmt;
                            finalVariantPrice = promoPrice < 0 ? 0 : promoPrice;

                            variantHasSale = true;
                            v.DiscountAmount = discountAmt;
                        }

                        // Gán giá vào thuộc tính NotMapped để dùng cho Cart/Checkout
                        v.CurrentPrice = finalVariantPrice;
                        v.IsOnSale = variantHasSale;
                    }
                }

                // --- C. TÍNH GIÁ HIỂN THỊ CHO PRODUCT (CARD) ---
                decimal displayPrice = p.BasePrice;
                bool productHasSale = false;
                decimal productDiscountAmt = 0;

                // Ưu tiên 1: Giá Promotion (Hệ thống tự động)
                if (bestPromo != null)
                {
                    decimal discountAmt = bestPromo.IsPercent
                        ? p.BasePrice * (bestPromo.DiscountValue / 100)
                        : bestPromo.DiscountValue;

                    displayPrice = p.BasePrice - discountAmt;
                    productHasSale = true;
                    productDiscountAmt = discountAmt;
                }
                // Ưu tiên 2: Giá Sale thủ công (Nếu rẻ hơn Promotion hoặc không có Promotion)
                else if (p.SalePrice.HasValue && p.SalePrice > 0 && p.SalePrice < p.BasePrice)
                {
                    if (displayPrice > p.SalePrice.Value)
                    {
                        displayPrice = p.SalePrice.Value;
                        productHasSale = true;
                        productDiscountAmt = p.BasePrice - p.SalePrice.Value;
                    }
                }

                // Chốt giá cuối cùng vào RAM (NotMapped Properties)
                p.CurrentPrice = displayPrice < 0 ? 0 : displayPrice;
                p.IsOnSale = productHasSale;
                p.DiscountAmount = productDiscountAmt;
            }
        }

        // ==================================================================================
        // 2. READ METHODS (BASIC)
        // ==================================================================================

        public async Task<IEnumerable<Product>> GetAllAsync(bool includeInactive = false)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(
                filter: p => !p.IsDeleted && (includeInactive || p.IsActive),
                includeProperties: "Categories,Variants" // Load cả Variants để tính giá
            );

            // [QUAN TRỌNG] Gọi hàm tính giá
            await ApplyPromotionLogic(products);

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetQuery()
                                .Include(p => p.Categories)
                                .Include(p => p.Variants)
                                    .ThenInclude(v => v.Inventory) // Load cả Inventory để check stock
                                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.IsDeleted) return null;

            // [QUAN TRỌNG] Gọi hàm tính giá chi tiết
            await ApplyPromotionLogic(new List<Product> { product });

            return product;
        }

        // ==================================================================================
        // 3. HOME PAGE SECTIONS (FEATURED / NEW / SALE)
        // ==================================================================================

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int take)
        {
            var products = await _unitOfWork.ProductRepository.GetQuery()
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => Guid.NewGuid()) // Random để thay đổi mỗi lần load
                .Take(take)
                .ToListAsync();

            await ApplyPromotionLogic(products);
            return products;
        }

        public async Task<IEnumerable<Product>> GetNewArrivalsAsync(int take)
        {
            var products = await _unitOfWork.ProductRepository.GetQuery()
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .ToListAsync();

            await ApplyPromotionLogic(products);
            return products;
        }

        public async Task<IEnumerable<Product>> GetOnSaleProductsAsync(int take)
        {
            // Lấy nhiều hơn số lượng cần (take * 3) để đảm bảo sau khi lọc vẫn đủ hiển thị
            var candidates = await _unitOfWork.ProductRepository.GetQuery()
                .Where(p => !p.IsDeleted && p.IsActive)
                .Take(take * 3)
                .ToListAsync();

            // Phải tính giá trước mới biết cái nào đang Sale
            await ApplyPromotionLogic(candidates);

            return candidates
                .Where(p => p.IsOnSale)
                .OrderBy(p => p.CurrentPrice) // Sắp xếp giá rẻ lên đầu
                .Take(take);
        }

        // ==================================================================================
        // 4. ADVANCED SEARCH & FILTER (TÍCH HỢP LOGIC TỪ CONTROLLER VÀO SERVICE)
        // ==================================================================================

        /// <summary>
        /// Hàm này thay thế hoàn toàn logic lọc trong Controller cũ.
        /// Xử lý: Search Text -> Category -> Brand -> Tính Giá -> Lọc Giá -> Sắp xếp
        /// </summary>
        public async Task<IEnumerable<Product>> GetFilteredProductsAsync(
            string? searchQuery,
            List<string>? brands,
            List<string>? priceRanges,
            string? sort,
            int? categoryId)
        {
            // Bước 1: Khởi tạo Query cơ bản
            var query = _unitOfWork.ProductRepository.GetQuery()
                        .Include(p => p.Categories)
                        .Include(p => p.Variants)
                        .Where(p => !p.IsDeleted && p.IsActive);

            // Bước 2: Lọc theo Category (Database)
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));
            }

            // Bước 3: Lọc theo Brand (Database)
            if (brands != null && brands.Any())
            {
                query = query.Where(p => brands.Contains(p.Brand));
            }

            // Bước 4: Tìm kiếm từ khóa (Database)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                string q = searchQuery.ToLower().Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(q) ||
                    p.Brand.ToLower().Contains(q) ||
                    (p.Description != null && p.Description.ToLower().Contains(q)) ||
                    p.Categories.Any(c => c.Name.ToLower().Contains(q)) ||
                    p.Variants.Any(v => v.SKU.ToLower().Contains(q))
                );
            }

            // Lấy dữ liệu từ DB về RAM
            var products = await query.ToListAsync();

            // Bước 5: [QUAN TRỌNG] TÍNH GIÁ THỰC TẾ
            // Phải làm bước này trước khi lọc giá và sắp xếp theo giá
            await ApplyPromotionLogic(products);

            // Bước 6: Lọc theo khoảng giá (In-Memory)
            // Vì CurrentPrice là biến tính toán, không query DB được
            if (priceRanges != null && priceRanges.Any())
            {
                var filteredByPrice = new List<Product>();

                if (priceRanges.Contains("under15"))
                    filteredByPrice.AddRange(products.Where(p => p.CurrentPrice < 1500000));

                if (priceRanges.Contains("15-25"))
                    filteredByPrice.AddRange(products.Where(p => p.CurrentPrice >= 1500000 && p.CurrentPrice <= 2500000));

                if (priceRanges.Contains("over25"))
                    filteredByPrice.AddRange(products.Where(p => p.CurrentPrice > 2500000));

                // Nếu có filter giá tùy chỉnh dạng "min-max"
                foreach (var range in priceRanges.Where(r => r.Contains("-") && r != "15-25"))
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0], out decimal min) &&
                        decimal.TryParse(parts[1], out decimal max))
                    {
                        filteredByPrice.AddRange(products.Where(p => p.CurrentPrice >= min && p.CurrentPrice <= max));
                    }
                }

                products = filteredByPrice.Distinct().ToList();
            }

            // Bước 7: Sắp xếp (In-Memory)
            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.CurrentPrice).ToList(),
                "price_desc" => products.OrderByDescending(p => p.CurrentPrice).ToList(),
                "name_asc" => products.OrderBy(p => p.Name).ToList(),
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                _ => products.OrderByDescending(p => p.CreatedAt).ToList() // Mặc định mới nhất
            };

            return products;
        }

        // ==================================================================================
        // 5. CUD OPERATIONS (CREATE / UPDATE / DELETE)
        // ==================================================================================

        public async Task CreateProductAsync(Product product, List<int> categoryIds)
        {
            product.Slug = SlugHelper.GenerateSlug(product.Name);
            product.CreatedAt = DateTime.UtcNow;
            product.IsDeleted = false;

            if (categoryIds != null && categoryIds.Any())
            {
                var categories = await _unitOfWork.CategoryRepository
                    .GetAllAsync(c => categoryIds.Contains(c.Id));
                product.Categories = categories.ToList();
            }

            await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateProductAsync(Product product, List<int> categoryIds)
        {
            var existingProduct = await _unitOfWork.ProductRepository.GetQuery()
                                    .Include(p => p.Categories)
                                    .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null) throw new Exception("Product not found");

            // Cập nhật thông tin cơ bản
            existingProduct.Name = product.Name;
            existingProduct.Slug = SlugHelper.GenerateSlug(product.Name);
            existingProduct.Brand = product.Brand;
            existingProduct.BasePrice = product.BasePrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.Description = product.Description;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                existingProduct.ImageUrl = product.ImageUrl;
            }

            // Cập nhật quan hệ Category
            existingProduct.Categories.Clear();
            if (categoryIds != null && categoryIds.Any())
            {
                var newCategories = await _unitOfWork.CategoryRepository
                    .GetAllAsync(c => categoryIds.Contains(c.Id));
                foreach (var cat in newCategories) existingProduct.Categories.Add(cat);
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                await _unitOfWork.SaveAsync();
            }
        }
    }
}