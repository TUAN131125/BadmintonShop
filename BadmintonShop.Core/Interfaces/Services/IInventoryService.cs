using System.Threading.Tasks;
using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IInventoryService
    {
        // [CẬP NHẬT] Thêm tham số importCost (giá nhập) để tính giá vốn trung bình
        Task IncreaseStockAsync(int variantId, int quantity, string reason, decimal importCost, InventoryActionType actionType = InventoryActionType.Import, int? userId = null);

        // Giữ nguyên logic giảm kho, bên trong sẽ thêm xử lý cảnh báo
        Task DecreaseStockAsync(int variantId, int quantity, string reason, int? userId, int? orderId = null);

        // [MỚI] Hàm Kiểm kê kho (Stocktake)
        // actualQuantity: Số lượng thực tế đếm được
        Task AdjustStockAsync(int variantId, int actualQuantity, string reason, int? userId);

        Task<Inventory> GetStockByVariantIdAsync(int variantId);
    }
}