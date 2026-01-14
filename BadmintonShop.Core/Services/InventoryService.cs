using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        // [CẤU HÌNH] Mức cảnh báo tồn kho chung cho toàn hệ thống
        // TODO: Sau này nên lấy từ Database (bảng GlobalSettings) thay vì hardcode số 5
        private const int GLOBAL_MIN_STOCK_THRESHOLD = 5;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. NHẬP KHO (Inbound) - Có tính toán Giá vốn bình quân
        public async Task IncreaseStockAsync(int variantId, int quantity, string reason, decimal importCost, InventoryActionType actionType = InventoryActionType.Import, int? userId = null)
        {
            if (quantity <= 0) throw new Exception("Quantity must be positive.");
            if (importCost < 0) throw new Exception("Import cost cannot be negative.");

            var inventory = await GetOrCreateInventoryAsync(variantId);

            // --- LOGIC TÍNH GIÁ VỐN BÌNH QUÂN (Moving Average Cost) ---
            // Công thức: (Giá trị cũ + Giá trị nhập mới) / Tổng số lượng mới
            decimal totalValueOld = inventory.Quantity * inventory.AverageCost;
            decimal totalValueNew = quantity * importCost;
            int totalQuantityNew = inventory.Quantity + quantity;

            if (totalQuantityNew > 0)
            {
                inventory.AverageCost = (totalValueOld + totalValueNew) / totalQuantityNew;
            }
            // ----------------------------------------------------------

            inventory.Quantity = totalQuantityNew;
            inventory.LastUpdated = DateTime.UtcNow;

            // Ghi Log
            inventory.Logs.Add(new InventoryLog
            {
                ProductVariantId = variantId,
                QuantityChange = quantity,
                CostPerUnit = importCost,        // Lưu giá nhập của lô này
                StockAfter = inventory.Quantity, // Lưu tồn sau khi nhập
                ActionType = actionType,
                Reason = reason,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();
        }

        // 2. XUẤT KHO (Outbound) - Có cảnh báo tồn kho
        public async Task DecreaseStockAsync(int variantId, int quantity, string reason, int? userId, int? orderId = null)
        {
            if (quantity <= 0) throw new Exception("Quantity must be positive.");

            var inventory = await _unitOfWork.InventoryRepository.GetByVariantIdAsync(variantId);

            if (inventory == null || inventory.Quantity < quantity)
                throw new Exception("Not enough stock available.");

            inventory.Quantity -= quantity;
            inventory.LastUpdated = DateTime.UtcNow;

            if (inventory.Logs == null) inventory.Logs = new List<InventoryLog>();

            // Ghi Log
            inventory.Logs.Add(new InventoryLog
            {
                ProductVariantId = variantId,
                QuantityChange = -quantity,
                CostPerUnit = inventory.AverageCost, // Khi xuất, giá vốn là giá bình quân hiện tại
                StockAfter = inventory.Quantity,     // Lưu tồn sau khi xuất
                ActionType = InventoryActionType.Export,
                Reason = reason,
                UserId = userId,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            });

            // --- LOGIC CẢNH BÁO TỒN KHO (LOW STOCK ALERT) ---
            // Ưu tiên MinStock riêng của sản phẩm, nếu null thì dùng Global Config
            int warningThreshold = inventory.MinStock ?? GLOBAL_MIN_STOCK_THRESHOLD;

            if (inventory.Quantity <= warningThreshold)
            {
                // TODO: Tích hợp NotificationService để gửi email/thông báo cho Admin
                // Ví dụ: _notificationService.SendLowStockAlert(variantId, inventory.Quantity);

                // Tạm thời log ra Console để debug
                Console.WriteLine($"[ALERT] Variant {variantId} is low on stock! Current: {inventory.Quantity}, Threshold: {warningThreshold}");
            }
            // ------------------------------------------------

            _unitOfWork.InventoryRepository.Update(inventory);
            await _unitOfWork.SaveAsync();
        }

        // 3. KIỂM KÊ KHO (Stocktake / Adjustment)
        public async Task AdjustStockAsync(int variantId, int actualQuantity, string reason, int? userId)
        {
            if (actualQuantity < 0) throw new Exception("Actual quantity cannot be negative.");

            var inventory = await GetOrCreateInventoryAsync(variantId);
            int currentSystemStock = inventory.Quantity;
            int diff = actualQuantity - currentSystemStock;

            if (diff == 0) return; // Không có sai lệch, không làm gì cả

            if (diff > 0)
            {
                // Thực tế > Hệ thống (Dư hàng) -> Cần NHẬP thêm
                // Khi tìm thấy hàng (dư), ta thường nhập với giá vốn hiện tại để không làm lệch giá bình quân
                await IncreaseStockAsync(variantId, diff, $"Stocktake Adjustment (Surplus): {reason}", inventory.AverageCost, InventoryActionType.Adjustment, userId);
            }
            else
            {
                // Thực tế < Hệ thống (Thiếu hàng/Mất mát) -> Cần XUẤT bớt
                // Dùng Math.Abs để chuyển số âm thành dương cho hàm Decrease
                await DecreaseStockAsync(variantId, Math.Abs(diff), $"Stocktake Adjustment (Loss): {reason}", userId);
            }
        }

        public async Task<Inventory> GetStockByVariantIdAsync(int variantId)
        {
            return await _unitOfWork.InventoryRepository.GetByVariantIdAsync(variantId);
        }

        private async Task<Inventory> GetOrCreateInventoryAsync(int variantId)
        {
            var inventory = await _unitOfWork.InventoryRepository.GetByVariantIdAsync(variantId);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductVariantId = variantId,
                    Quantity = 0,
                    AverageCost = 0, // Khởi tạo giá vốn là 0
                    Logs = new List<InventoryLog>()
                };
                await _unitOfWork.InventoryRepository.AddAsync(inventory);
            }
            else if (inventory.Logs == null)
            {
                inventory.Logs = new List<InventoryLog>();
            }

            return inventory;
        }
    }
}