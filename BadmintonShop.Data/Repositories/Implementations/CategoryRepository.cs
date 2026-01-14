using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Repositories.Implementations
{
    public class CategoryRepository
        : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentId == null && !c.IsDeleted)
                .Include(c => c.Children.Where(ch => !ch.IsDeleted))
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetChildrenAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId && !c.IsDeleted)
                .Include(c => c.Children.Where(ch => !ch.IsDeleted))
                .ToListAsync();
        }
    }
}