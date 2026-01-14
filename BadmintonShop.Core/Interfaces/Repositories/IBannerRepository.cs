using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Repositories
{
    public interface IBannerRepository : IGenericRepository<Banner>
    {
        Task<int> GetMaxOrderAsync();
    }
}
