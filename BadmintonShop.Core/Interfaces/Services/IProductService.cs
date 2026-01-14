using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IProductService
    {
        // Get
        Task<IEnumerable<Product>> GetAllAsync(bool includeInactive = false);
        Task<Product?> GetByIdAsync(int id);
        //Tính giá cho bất kỳ danh sách sản phẩm nào(Dùng cho Cart & Order)
        Task ApplyPromotionLogic(IEnumerable<Product> products);

        // Create & Update: Nhận Entity và List CategoryId
        Task CreateProductAsync(Product product, List<int> categoryIds);
        Task UpdateProductAsync(Product product, List<int> categoryIds);

        // Delete & Others
        Task SoftDeleteAsync(int id);

        // Helper queries
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int take);
        Task<IEnumerable<Product>> GetNewArrivalsAsync(int take);
        Task<IEnumerable<Product>> GetOnSaleProductsAsync(int take);
    }
}