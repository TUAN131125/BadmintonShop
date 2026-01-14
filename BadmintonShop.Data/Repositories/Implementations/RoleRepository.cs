using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using BadmintonShop.Data.Repositories.Implementations;

namespace BadmintonShop.Data.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext ctx) : base(ctx) { }
    }
}
