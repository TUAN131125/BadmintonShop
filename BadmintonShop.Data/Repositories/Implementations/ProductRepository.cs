using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Product> GetProductDetailAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}