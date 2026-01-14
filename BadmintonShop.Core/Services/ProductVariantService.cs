using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductVariantService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
            if (product == null || product.IsDeleted)
                throw new Exception("Product not found.");

            // Lấy variant kèm Inventory.
            // Lưu ý: Admin có thể cần thấy cả variant đã tắt (IsActive=false) để quản lý.
            // Nếu là API cho khách hàng, bạn nên filter thêm && v.IsActive == true
            return await _unitOfWork.ProductVariantRepository
                .GetAllAsync(v => !v.IsDeleted && v.ProductId == productId,
                             includeProperties: "Inventory");
        }

        public async Task<ProductVariant> GetByIdAsync(int id)
        {
            var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(id);
            if (variant == null || variant.IsDeleted)
                return null;

            return variant;
        }

        public async Task CreateAsync(ProductVariant variant)
        {
            ValidateVariant(variant);

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(variant.ProductId);
            if (product == null || product.IsDeleted)
                throw new Exception("Invalid parent product.");

            // Kiểm tra trùng SKU
            var existingSku = await _unitOfWork.ProductVariantRepository
                .GetAllAsync(v => v.SKU == variant.SKU && !v.IsDeleted);

            if (existingSku.Any())
                throw new Exception($"SKU code '{variant.SKU}' already exists.");

            variant.CreatedAt = DateTime.UtcNow;
            variant.IsDeleted = false;
            variant.IsActive = true; // Mặc định khi tạo mới là Đang bán

            // 1. Thêm Variant
            await _unitOfWork.ProductVariantRepository.AddAsync(variant);

            // 2. TỰ ĐỘNG TẠO INVENTORY (KHO) CHUẨN HÓA
            var inventory = new Inventory
            {
                ProductVariant = variant,
                Quantity = 0,
                AverageCost = 0, // [MỚI] Khởi tạo giá vốn = 0
                MinStock = null, // [MỚI] Null = Sử dụng cấu hình chung (Global Config)
                Logs = new List<InventoryLog>()
            };

            await _unitOfWork.InventoryRepository.AddAsync(inventory);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(ProductVariant variant)
        {
            var existing = await _unitOfWork.ProductVariantRepository.GetByIdAsync(variant.Id);
            if (existing == null || existing.IsDeleted)
                throw new Exception("Variant not found.");

            ValidateVariant(variant);

            var skuConflict = await _unitOfWork.ProductVariantRepository
                .GetAllAsync(v => v.SKU == variant.SKU &&
                                  v.Id != variant.Id &&
                                  !v.IsDeleted);

            if (skuConflict.Any())
                throw new Exception($"SKU code '{variant.SKU}' is already used by another product.");

            // Cập nhật thông tin
            existing.SKU = variant.SKU;
            existing.AttributeName = variant.AttributeName;
            existing.AttributeValue = variant.AttributeValue;
            existing.Price = variant.Price;

            // [MỚI] Cho phép cập nhật trạng thái kinh doanh (Bật/Tắt)
            existing.IsActive = variant.IsActive;

            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariantRepository.Update(existing);
            await _unitOfWork.SaveAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            // [LOGIC MỚI] Kiểm tra ràng buộc trước khi xóa
           /* var inventory = await _unitOfWork.InventoryRepository.GetByVariantIdAsync(id);

            // Nếu đã từng nhập hàng (Giá vốn > 0) hoặc đang còn tồn (Quantity > 0)
            // thì KHÔNG được xóa, mà phải yêu cầu người dùng Tắt (Deactivate)
            if (inventory != null && (inventory.Quantity > 0 || inventory.AverageCost > 0))
            {
                throw new Exception("This product has inventory data or remaining stock. Cannot delete! Please choose 'Deactivate' (IsActive = false) instead of deleting.");
            }*/

            var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(id);
            if (variant != null)
            {
                variant.IsDeleted = true;
                variant.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.ProductVariantRepository.Update(variant);
                await _unitOfWork.SaveAsync();
            }
        }

        private void ValidateVariant(ProductVariant variant)
        {
            if (string.IsNullOrWhiteSpace(variant.SKU))
                throw new Exception("SKU code cannot be empty.");

            if (string.IsNullOrWhiteSpace(variant.AttributeName))
                throw new Exception("Attribute name cannot be empty.");

            if (string.IsNullOrWhiteSpace(variant.AttributeValue))
                throw new Exception("Attribute value cannot be empty.");

            if (variant.Price < 0)
                throw new Exception("Price cannot be negative.");
        }

        public async Task<IEnumerable<ProductVariant>> GetAllAsync()
        {
            return await _unitOfWork.ProductVariantRepository.GetQuery()
                        .Include(v => v.Inventory)
                        .ToListAsync();
        }
    }
}