using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Inventory> GetByVariantIdAsync(int variantId)
        {
            // Sử dụng Include để lấy kèm danh sách Logs nhằm tránh lỗi NullReference khi ghi log mới
            return await _context.Inventories
                .Include(i => i.Logs)
                .FirstOrDefaultAsync(i => i.ProductVariantId == variantId);
        }
    }
}