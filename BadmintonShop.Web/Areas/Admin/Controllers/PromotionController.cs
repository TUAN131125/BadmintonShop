using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PromotionController : BaseAdminController
    {
        private readonly IPromotionService _promotionService;
        private readonly IUnitOfWork _unitOfWork;

        public PromotionController(IPromotionService promotionService, IUnitOfWork unitOfWork)
        {
            _promotionService = promotionService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _unitOfWork.PromotionRepository.GetQuery()
                                .OrderByDescending(p => p.CreatedAt)
                                .ToListAsync();
            return View(list);
        }

        // ==========================================
        // 2. CREATE: TẠO MỚI
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            // [FIX LỖI THỜI GIAN]
            // Lấy thời gian hiện tại nhưng cắt bỏ giây và mili-giây để form sạch đẹp, dễ nhập
            var now = DateTime.Now;
            var cleanNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            return View(new Promotion
            {
                StartDate = cleanNow,
                EndDate = cleanNow.AddDays(7),
                IsActive = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            // [FIX QUAN TRỌNG]
            // 1. Bỏ qua validation ngày tháng (như đã làm)
            ModelState.Remove("StartDate");
            ModelState.Remove("EndDate");

            // 2. [THÊM DÒNG NÀY] Bỏ qua validation danh sách sản phẩm
            // Vì lúc tạo mới chưa có sản phẩm nào, nên ta phải bỏ qua lỗi này
            ModelState.Remove("PromotionProducts");

            // 3. Xử lý ép kiểu ngày tháng thủ công (Code cũ giữ nguyên)
            if (DateTime.TryParse(Request.Form["StartDate"], out var sDate))
            {
                promotion.StartDate = sDate;
            }

            if (DateTime.TryParse(Request.Form["EndDate"], out var eDate))
            {
                promotion.EndDate = eDate;
            }

            if (promotion.StartDate >= promotion.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                // Khởi tạo danh sách rỗng để tránh lỗi null khi lưu vào DB (nếu cần)
                promotion.PromotionProducts = new List<PromotionProduct>();

                await _promotionService.CreatePromotionAsync(promotion);
                TempData["SuccessMessage"] = "Đã tạo chương trình khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu vẫn lỗi, hiển thị lại View để xem lỗi gì tiếp theo
            return View(promotion);
        }

        // ==========================================
        // 3. EDIT: CẬP NHẬT
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(id);
            if (promotion == null) return NotFound();

            // [FIX LỖI THỜI GIAN KHI EDIT]
            // Khi load từ DB lên, nó có thể chứa mili-giây. Cần cắt bỏ trước khi đưa vào View.
            // Nếu không cắt, thẻ input sẽ báo lỗi validation.
            promotion.StartDate = new DateTime(promotion.StartDate.Year, promotion.StartDate.Month, promotion.StartDate.Day, promotion.StartDate.Hour, promotion.StartDate.Minute, promotion.StartDate.Second);
            promotion.EndDate = new DateTime(promotion.EndDate.Year, promotion.EndDate.Month, promotion.EndDate.Day, promotion.EndDate.Hour, promotion.EndDate.Minute, promotion.EndDate.Second);

            return View(promotion);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Promotion promotion)
        {
            // [FIX LỖI VALIDATION]
            // 1. Bỏ qua lỗi ngày tháng (do định dạng)
            ModelState.Remove("StartDate");
            ModelState.Remove("EndDate");

            // 2. Bỏ qua lỗi danh sách sản phẩm (vì form Edit không gửi list này lên)
            ModelState.Remove("PromotionProducts");

            // 3. Ép kiểu ngày tháng thủ công từ dữ liệu form gửi lên
            if (DateTime.TryParse(Request.Form["StartDate"], out var sDate))
            {
                promotion.StartDate = sDate;
            }

            if (DateTime.TryParse(Request.Form["EndDate"], out var eDate))
            {
                promotion.EndDate = eDate;
            }

            // Kiểm tra logic ngày
            if (promotion.StartDate >= promotion.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                // Lấy entity cũ từ DB
                var existing = await _unitOfWork.PromotionRepository.GetByIdAsync(promotion.Id);
                if (existing == null) return NotFound();

                // Map dữ liệu mới vào
                existing.Name = promotion.Name;
                existing.Description = promotion.Description;
                existing.StartDate = promotion.StartDate;
                existing.EndDate = promotion.EndDate;
                existing.DiscountValue = promotion.DiscountValue;
                existing.IsPercent = promotion.IsPercent;
                existing.IsActive = promotion.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                // Lưu thay đổi
                _unitOfWork.PromotionRepository.Update(existing);
                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = "Cập nhật khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu vẫn lỗi thì hiện lại View để debug
            return View(promotion);
        }

        // ==========================================
        // 4. DELETE: XÓA
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(id);
            if (promotion != null)
            {
                _unitOfWork.PromotionRepository.Delete(promotion);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Đã xóa chương trình khuyến mãi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. SELECT PRODUCTS (Không thay đổi)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SelectProducts(int id, string? keyword, string? brand, int? categoryId)
        {
            var promotion = await _unitOfWork.PromotionRepository.GetPromotionWithProductsAsync(id);
            if (promotion == null) return NotFound();

            var vm = new PromotionProductSelectVM
            {
                PromotionId = promotion.Id,
                PromotionName = promotion.Name,
                Keyword = keyword,
                Brand = brand,
                CategoryId = categoryId
            };

            var brands = await _unitOfWork.ProductRepository.GetQuery()
                                .Where(p => !string.IsNullOrEmpty(p.Brand))
                                .Select(p => p.Brand)
                                .Distinct()
                                .ToListAsync();
            ViewBag.Brands = new SelectList(brands);
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var query = _unitOfWork.ProductRepository.GetQuery()
                        .Include(p => p.Categories)
                        .Where(p => !p.IsDeleted && p.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                var k = keyword.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(k));
            }
            if (!string.IsNullOrEmpty(brand)) query = query.Where(p => p.Brand == brand);
            if (categoryId.HasValue) query = query.Where(p => p.Categories.Any(c => c.Id == categoryId));

            var products = await query.Take(50).ToListAsync();
            var existingIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();

            vm.Products = products.Select(p => new ProductCheckItemVM
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand,
                Price = p.BasePrice,
                ImageUrl = p.ImageUrl,
                IsAlreadyInPromotion = existingIds.Contains(p.Id)
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddProducts(int promotionId, List<int> selectedProductIds)
        {
            if (selectedProductIds != null && selectedProductIds.Any())
            {
                await _promotionService.AddProductsToPromotionAsync(promotionId, selectedProductIds);
                TempData["SuccessMessage"] = $"Added {selectedProductIds.Count} products to promotion successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "No products selected.";
            }
            return RedirectToAction(nameof(SelectProducts), new { id = promotionId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int promotionId, int productId)
        {
            await _promotionService.RemoveProductsFromPromotionAsync(promotionId, new List<int> { productId });
            TempData["SuccessMessage"] = "Removed product from promotion.";
            return RedirectToAction(nameof(SelectProducts), new { id = promotionId });
        }
    }
}