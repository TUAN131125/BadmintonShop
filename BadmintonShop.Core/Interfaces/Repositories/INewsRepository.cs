using BadmintonShop.Core.Entities;
namespace BadmintonShop.Core.Interfaces.Repositories
{
    public interface INewsRepository : IGenericRepository<News>
    {
        // Có thể thêm hàm tìm theo Slug nếu cần
        Task<News> GetBySlugAsync(string slug);

    }
}