using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllAsync();

        Task<Role?> GetByNameAsync(string roleName);   // <-- thêm dòng này
    }
}
