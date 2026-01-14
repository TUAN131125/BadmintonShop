using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductVariantController : BaseAdminController
    {
        private readonly IProductVariantService _variantService;
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;

        public ProductVariantController(
            IProductVariantService variantService,
            IProductService productService,
            IInventoryService inventoryService)
        {
            _variantService = variantService;
            _productService = productService;
            _inventoryService = inventoryService;
        }

        // 1. DANH SÁCH BIẾN THỂ
        [HttpGet]
        public async Task<IActionResult> Index(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var variants = await _variantService.GetByProductIdAsync(productId);

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = productId;

            // Map Entity -> ViewModel
            var vm = variants.Select(v => new ProductVariantVM
            {
                Id = v.Id,
                ProductId = v.ProductId,
                ProductName = product.Name,
                SKU = v.SKU,
                AttributeName = v.AttributeName,
                AttributeValue = v.AttributeValue,

                // [SỬA LỖI] Lấy Giá vốn từ bảng Inventory (AverageCost) thay vì bảng Variant
                ImportPrice = v.Inventory != null ? v.Inventory.AverageCost : 0,

                Price = v.Price,
                Stock = v.Inventory != null ? v.Inventory.Quantity : 0,

                // [MỚI] Hiển thị trạng thái
                IsActive = v.IsActive
            }).ToList();

            return View(vm);
        }

        // 2. TẠO MỚI (CREATE)
        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var vm = new ProductVariantVM
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.BasePrice
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVariantVM vm)
        {
            if (ModelState.IsValid)
            {
                // BƯỚC 1: Tạo Variant
                var variant = new ProductVariant
                {
                    ProductId = vm.ProductId,
                    AttributeName = vm.AttributeName,
                    AttributeValue = vm.AttributeValue,

                    // [SỬA LỖI] Đã xóa dòng ImportPrice = vm.ImportPrice vì Entity không còn trường này

                    Price = vm.Price,
                    SKU = vm.SKU ?? $"{vm.ProductId}-{vm.AttributeValue}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                    IsActive = true // Mặc định kích hoạt
                };

                // Lưu Variant để lấy ID
                await _variantService.CreateAsync(variant);

                // BƯỚC 2: Tạo Inventory ban đầu (nếu có nhập số lượng)
                if (vm.Quantity > 0)
                {
                    // Giả lập UserId (cần lấy từ Identity trong thực tế)
                    int? currentUserId = null;

                    // [SỬA LỖI] Thêm tham số importCost (lấy từ ViewModel người dùng nhập)
                    await _inventoryService.IncreaseStockAsync(
                        variantId: variant.Id,
                        quantity: vm.Quantity,
                        reason: "Initial stock creation",
                        importCost: vm.ImportPrice, // <-- THÊM MỚI: Truyền giá nhập vào để tính AverageCost
                        userId: currentUserId
                    );
                }

                TempData["Success"] = "Variant and Inventory created successfully!";
                return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
            }

            return View(vm);
        }

        // 3. CẬP NHẬT (EDIT)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var v = await _variantService.GetByIdAsync(id);
            if (v == null) return NotFound();

            var vm = new ProductVariantVM
            {
                Id = v.Id,
                ProductId = v.ProductId,
                SKU = v.SKU,
                AttributeName = v.AttributeName,
                AttributeValue = v.AttributeValue,

                // [SỬA LỖI] Lấy giá vốn hiện tại từ Inventory để hiển thị
                ImportPrice = v.Inventory != null ? v.Inventory.AverageCost : 0,

                Price = v.Price,
                Stock = v.Inventory != null ? v.Inventory.Quantity : 0,
                IsActive = v.IsActive // [MỚI] Map trạng thái
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductVariantVM vm)
        {
            if (ModelState.IsValid)
            {
                var variant = await _variantService.GetByIdAsync(vm.Id);
                if (variant == null) return NotFound();

                // Map dữ liệu
                variant.AttributeName = vm.AttributeName;
                variant.AttributeValue = vm.AttributeValue;
                variant.Price = vm.Price;
                variant.SKU = vm.SKU;
                variant.IsActive = vm.IsActive; // [MỚI] Cho phép bật/tắt kinh doanh

                // [SỬA LỖI] Đã xóa dòng cập nhật ImportPrice. 
                // Lý do: Giá vốn (AverageCost) được tính tự động khi nhập kho, 
                // không được sửa tay trực tiếp ở đây để tránh lệch số liệu kế toán.

                await _variantService.UpdateAsync(variant);

                TempData["Success"] = "Variant updated successfully!";
                return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
            }
            return View(vm);
        }

        // 4. XÓA (DELETE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string returnUrl = null)
        {
            try
            {
                // Gọi hàm SoftDelete, bên Service đã có logic chặn nếu còn tồn kho
                await _variantService.SoftDeleteAsync(id);
                TempData["Success"] = "Variant deactivated/deleted successfully.";
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu Service ném ra (VD: Không cho xóa vì còn hàng)
                TempData["Error"] = ex.Message;
            }

            // Redirect logic (đơn giản hóa để tránh query DB lại)
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

            // Nếu không có returnUrl, quay về trang danh sách (cần productId, nhưng ở đây ta redirect về trang Product Index tạm)
            return RedirectToAction("Index", "Product");
        }

        // 5. ĐIỀU CHỈNH KHO (ADJUST STOCK)
        // Đây là trang riêng để nhập kho/kiểm kê
        [HttpGet]
        public async Task<IActionResult> AdjustStock(int variantId)
        {
            // Code xử lý view nhập kho sẽ làm sau
            return View();
        }
    }
}