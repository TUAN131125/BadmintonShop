using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;

namespace BadmintonShop.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _uow;

        public RoleService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _uow.RoleRepository.GetAllAsync();
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return (await _uow.RoleRepository.GetAllAsync())
                .FirstOrDefault(r => r.Name == roleName);
        }
    }
}
