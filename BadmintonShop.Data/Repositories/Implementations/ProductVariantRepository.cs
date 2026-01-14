using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class ProductVariantRepository
        : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Include(v => v.Inventory)
                .Where(v => v.ProductId == productId)
                .ToListAsync();
        }
    }
}
