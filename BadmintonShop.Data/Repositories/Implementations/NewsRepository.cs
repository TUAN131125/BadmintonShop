using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using BadmintonShop.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories
{
    public class NewsRepository : GenericRepository<News>, INewsRepository
    {
        public NewsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<News> GetBySlugAsync(string slug)
        {
            return await _context.News.FirstOrDefaultAsync(x => x.Slug == slug);
        }
    }
}