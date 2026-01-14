using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface INewsService
    {
        Task<IEnumerable<News>> GetAllAsync(); // Cho Admin
        Task<IEnumerable<News>> GetPublishedAsync(); // Cho Customer
        Task<News> GetByIdAsync(int id);
        Task CreateAsync(News news);
        Task UpdateAsync(News news);
        Task DeleteAsync(int id);
    }
}