using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using BadmintonShop.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BadmintonShop.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext ctx) : base(ctx) { }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == username);
        }
    }
}
