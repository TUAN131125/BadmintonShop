using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Order> GetOrderDetailAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
