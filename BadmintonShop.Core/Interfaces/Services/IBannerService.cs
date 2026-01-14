using BadmintonShop.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IBannerService
    {
        Task<IEnumerable<Banner>> GetAllAsync();

        Task<Banner?> GetByIdAsync(int id);

        Task CreateAsync(Banner banner, string user);
        Task UpdateAsync(Banner banner, string user);

        Task ToggleAsync(int id);
        Task SoftDeleteAsync(int id);
    }
}