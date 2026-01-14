using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class BannerRepository : GenericRepository<Banner>, IBannerRepository
    {
        public BannerRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<int> GetMaxOrderAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .MaxAsync(x => (int?)x.OrderIndex) ?? 0;
        }
    }
}
