using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Promotion?> GetActivePromotionByProductIdAsync(int productId)
        {
            var now = DateTime.UtcNow; // Hoặc DateTime.Now tùy múi giờ server

            // Tìm promotion nào chứa productId này VÀ đang trong thời gian hiệu lực
            return await _context.PromotionProducts
                .Where(pp => pp.ProductId == productId
                             && pp.Promotion.IsActive
                             && !pp.Promotion.IsDeleted
                             && pp.Promotion.StartDate <= now
                             && pp.Promotion.EndDate >= now)
                .Select(pp => pp.Promotion)
                .OrderByDescending(p => p.DiscountValue) // Ưu tiên khuyến mãi giảm nhiều nhất
                .FirstOrDefaultAsync();
        }

        public async Task<Promotion?> GetPromotionWithProductsAsync(int promotionId)
        {
            return await _context.Promotions
                .Include(p => p.PromotionProducts)
                .FirstOrDefaultAsync(p => p.Id == promotionId);
        }
    }
}