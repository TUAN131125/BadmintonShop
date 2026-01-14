using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IPromotionService
    {
        Task CreatePromotionAsync(Promotion promotion);

        Task AddProductsToPromotionAsync(int promotionId, List<int> productIds);

        // [MỚI] Hàm xóa một danh sách sản phẩm khỏi Promotion (nếu lỡ tay add nhầm)
        Task RemoveProductsFromPromotionAsync(int promotionId, List<int> productIds);

        // Hàm tính giá cuối cùng (để hiển thị ra ngoài)
        decimal CalculateDiscountedPrice(decimal originalPrice, Promotion? activePromotion);
    }

    // Class chứa tiêu chí lọc (DTO)
    public class PromotionFilterCriteria
    {
        public string? Brand { get; set; }      // Lọc theo thương hiệu (Yonex, Lining...)
        public int? CategoryId { get; set; }    // Lọc theo danh mục (Vợt, Giày...)
        public string? Keyword { get; set; }    // Lọc theo tên/mô tả
    }
}