using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Core.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PromotionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreatePromotionAsync(Promotion promotion)
        {
            if (promotion.StartDate >= promotion.EndDate)
                throw new Exception("Start date must be before End date.");

            promotion.CreatedAt = DateTime.UtcNow;
            promotion.IsDeleted = false;

            await _unitOfWork.PromotionRepository.AddAsync(promotion);
            await _unitOfWork.SaveAsync();
        }

        // ==========================================================
        // [LOGIC MỚI] THÊM SẢN PHẨM VÀO KHUYẾN MÃI TỪ LIST ID
        // ==========================================================
        public async Task AddProductsToPromotionAsync(int promotionId, List<int> productIds)
        {
            if (productIds == null || !productIds.Any()) return;

            // 1. Lấy Promotion hiện tại và các sản phẩm đã có bên trong
            var promotion = await _unitOfWork.PromotionRepository.GetPromotionWithProductsAsync(promotionId);

            if (promotion == null) throw new Exception("Promotion not found");

            // Đảm bảo list không null để tránh lỗi
            if (promotion.PromotionProducts == null)
                promotion.PromotionProducts = new List<PromotionProduct>();

            // 2. Lọc bỏ những ID đã tồn tại trong promotion này (Tránh trùng lặp)
            // Lấy ra danh sách ID đang có trong DB
            var existingIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();

            // Chỉ lấy những ID mới (nằm trong productIds NHƯNG KHÔNG nằm trong existingIds)
            var newIds = productIds.Except(existingIds).ToList();

            // 3. Tạo Entity và Add vào
            foreach (var id in newIds)
            {
                promotion.PromotionProducts.Add(new PromotionProduct
                {
                    PromotionId = promotionId,
                    ProductId = id
                });
            }

            // 4. Lưu thay đổi
            if (newIds.Any())
            {
                await _unitOfWork.SaveAsync();
            }
        }

        // ==========================================================
        // [LOGIC MỚI] XÓA SẢN PHẨM KHỎI KHUYẾN MÃI
        // ==========================================================
        public async Task RemoveProductsFromPromotionAsync(int promotionId, List<int> productIds)
        {
            if (productIds == null || !productIds.Any()) return;

            var promotion = await _unitOfWork.PromotionRepository.GetPromotionWithProductsAsync(promotionId);
            if (promotion == null || promotion.PromotionProducts == null) return;

            // Tìm các item trong bảng trung gian khớp với danh sách ID cần xóa
            var itemsToRemove = promotion.PromotionProducts
                                .Where(pp => productIds.Contains(pp.ProductId))
                                .ToList();

            if (itemsToRemove.Any())
            {
                foreach (var item in itemsToRemove)
                {
                    // Xóa khỏi danh sách liên kết
                    promotion.PromotionProducts.Remove(item);
                }
                await _unitOfWork.SaveAsync();
            }
        }

        public decimal CalculateDiscountedPrice(decimal originalPrice, Promotion? promotion)
        {
            if (promotion == null) return originalPrice;

            decimal discountAmount = 0;
            if (promotion.IsPercent)
            {
                discountAmount = originalPrice * (promotion.DiscountValue / 100);
            }
            else
            {
                discountAmount = promotion.DiscountValue;
            }

            decimal finalPrice = originalPrice - discountAmount;
            return finalPrice < 0 ? 0 : finalPrice;
        }
    }
}