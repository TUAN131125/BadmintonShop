using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        // ===== CUSTOMER / MENU =====
        Task<IEnumerable<Category>> GetCategoryTreeAsync();
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetChildrenAsync(int parentId);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        // ===== ADMIN / CRUD =====
        Task<Category> GetByIdAsync(int id);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}
