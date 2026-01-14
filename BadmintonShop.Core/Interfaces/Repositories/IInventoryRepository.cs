using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Repositories
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        // Lấy thông tin kho kèm theo lịch sử log
        Task<Inventory> GetByVariantIdAsync(int variantId);
    }
}