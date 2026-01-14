using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Repositories
{
    public interface IPromotionRepository : IGenericRepository<Promotion>
    {
        // Hàm lấy danh sách Promotion đang chạy kèm theo sản phẩm
        Task<Promotion?> GetActivePromotionByProductIdAsync(int productId);

        // Hàm lấy Promotion kèm list ProductId bên trong (để hiển thị)
        Task<Promotion?> GetPromotionWithProductsAsync(int promotionId);
    }
}