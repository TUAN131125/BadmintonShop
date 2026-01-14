using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Cần thiết để dùng .Include()
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryController : BaseAdminController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IUnitOfWork _uow;

        public InventoryController(IInventoryService inventoryService, IUnitOfWork uow)
        {
            _inventoryService = inventoryService;
            _uow = uow;
        }

        // 1. TRANG QUẢN LÝ KHO (DASHBOARD)
        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu Variant kèm theo thông tin Kho
            var variants = await _uow.ProductVariantRepository.GetAllAsync(includeProperties: "Product,Inventory");

            // Map sang ViewModel
            var model = variants.Select(v => new InventoryIndexVM
            {
                Id = v.Id,
                ProductName = v.Product?.Name ?? "Unknown",
                SKU = v.SKU,
                AttributeName = v.AttributeName,
                AttributeValue = v.AttributeValue,

                // Hiển thị số lượng và giá vốn bình quân
                Quantity = v.Inventory?.Quantity ?? 0,
                AverageCost = v.Inventory?.AverageCost ?? 0,

                // Logic hiển thị cảnh báo trên giao diện (Frontend sẽ tô đỏ nếu true)
                // Nếu MinStock riêng null thì dùng Global (giả sử là 5)
                IsLowStock = (v.Inventory?.Quantity ?? 0) <= (v.Inventory?.MinStock ?? 5)
            }).ToList();

            // Tạo Dropdown cho Modal Nhập kho/Kiểm kê
            ViewBag.VariantList = variants.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Product?.Name} ({v.AttributeValue}) - Tồn: {v.Inventory?.Quantity ?? 0}"
            }).ToList();

            return View(model);
        }

        // 2. NHẬP KHO (INBOUND) - ĐÃ SỬA LỖI VÀ NÂNG CẤP
        [HttpPost]
        public async Task<IActionResult> ImportStock(InventoryImportVM model)
        {
            // Lưu ý: Bạn cần update InventoryImportVM thêm trường ImportPrice nhé
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input data.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Lấy ID Admin đang đăng nhập
                int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // [FIX LỖI & NÂNG CẤP]
                // Gọi Service với đầy đủ tham số mới để tính giá vốn bình quân
                await _inventoryService.IncreaseStockAsync(
                    variantId: model.ProductVariantId,
                    quantity: model.Quantity,
                    reason: model.Reason,
                    importCost: model.ImportPrice, // <-- THAM SỐ MỚI: Giá nhập của lô này
                    actionType: InventoryActionType.Import,
                    userId: adminId
                );

                TempData["Success"] = "Imported stock successfully! Average cost updated.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. KIỂM KÊ KHO (STOCKTAKE / ADJUSTMENT) - TÍNH NĂNG MỚI
        [HttpPost]
        public async Task<IActionResult> AdjustStock(int productVariantId, int actualQuantity, string reason)
        {
            try
            {
                int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Gọi Service Kiểm kê: Hệ thống tự tính chênh lệch để Nhập thêm hoặc Xuất bớt
                await _inventoryService.AdjustStockAsync(
                    variantId: productVariantId,
                    actualQuantity: actualQuantity, // Số thực tế đếm được
                    reason: reason,
                    userId: adminId
                );

                TempData["Success"] = "Inventory adjusted based on stocktake.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Adjustment failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // 4. LỊCH SỬ GIAO DỊCH (HISTORY)
        public async Task<IActionResult> History(int id)
        {
            // Lấy Inventory kèm theo Logs và thông tin Variant/Product
            // Cần truy cập DbSet trực tiếp hoặc Repository phải hỗ trợ Include sâu
            var inventory = await _uow.InventoryRepository.GetQuery()
                .Include(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Include(i => i.Logs)
                    .ThenInclude(l => l.User) // Để hiện tên người thực hiện
                .FirstOrDefaultAsync(i => i.ProductVariantId == id);

            if (inventory == null)
            {
                TempData["Error"] = "Inventory data not found.";
                return RedirectToAction(nameof(Index));
            }

            // Sắp xếp log mới nhất lên đầu
            inventory.Logs = inventory.Logs.OrderByDescending(l => l.CreatedAt).ToList();

            return View(inventory);
        }

        // 1. Trang hiển thị danh sách "Rác" cần dọn
        [HttpGet]
        public async Task<IActionResult> Cleanup()
        {
            // Lấy tất cả Variant ĐÃ BỊ XÓA (IsDeleted = true) nhưng vẫn còn dữ liệu trong bảng Inventory
            var deletedVariants = await _uow.ProductVariantRepository.GetAllAsync(
                filter: v => v.IsDeleted,
                includeProperties: "Product,Inventory"
            );

            // Map sang ViewModel để hiển thị
            var model = deletedVariants.Select(v => new InventoryIndexVM
            {
                Id = v.Id, // VariantId
                ProductName = v.Product?.Name ?? "Unknown (Orphaned)",
                SKU = v.SKU,
                AttributeName = v.AttributeName,
                AttributeValue = v.AttributeValue,
                Quantity = v.Inventory?.Quantity ?? 0,
                AverageCost = v.Inventory?.AverageCost ?? 0
            }).ToList();

            return View(model);
        }

        // 2. Xử lý Xóa vĩnh viễn (Hard Delete)
        [HttpPost]
        public async Task<IActionResult> CleanupConfirmed(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["Error"] = "Chưa chọn sản phẩm nào để xóa.";
                return RedirectToAction(nameof(Cleanup));
            }

            try
            {
                int count = 0;
                foreach (var id in selectedIds)
                {
                    // 1. Lấy Inventory
                    var inventory = await _uow.InventoryRepository.GetByVariantIdAsync(id);
                    if (inventory != null)
                    {
                        // Xóa Inventory (Logs sẽ tự xóa nếu cấu hình Cascade Delete trong DbContext, 
                        // nếu chưa thì phải xóa Logs thủ công tại đây trước)
                        _uow.InventoryRepository.Delete(inventory);
                    }

                    // 2. (Tùy chọn) Nếu muốn xóa bay màu luôn cả Variant khỏi DB (không giữ Soft Delete nữa)
                    var variant = await _uow.ProductVariantRepository.GetByIdAsync(id);
                    if (variant != null)
                    {
                        _uow.ProductVariantRepository.Delete(variant);
                    }

                    count++;
                }

                await _uow.SaveAsync();
                TempData["Success"] = $"Đã dọn dẹp vĩnh viễn {count} bản ghi rác.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }

            return RedirectToAction(nameof(Cleanup));
        }
    }
}