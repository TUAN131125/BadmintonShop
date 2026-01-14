using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Services
{
    public class BannerService : IBannerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BannerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Banner>> GetAllAsync()
        {
            // Lấy tất cả banner chưa bị xóa, sắp xếp theo thứ tự hiển thị
            var banners = await _unitOfWork.BannerRepository.GetAllAsync(x => !x.IsDeleted);
            return banners.OrderBy(b => b.OrderIndex);
        }

        public async Task<Banner?> GetByIdAsync(int id)
        {
            var banner = await _unitOfWork.BannerRepository.GetByIdAsync(id);
            if (banner == null || banner.IsDeleted)
                return null;

            return banner;
        }

        public async Task CreateAsync(Banner banner, string user)
        {
            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(banner.Title))
                throw new Exception("Banner Title is required");

            if (string.IsNullOrWhiteSpace(banner.ImageUrl))
                throw new Exception("Banner Image is required");

            // Tự động tính OrderIndex: Lấy max hiện tại + 1
            var currentMaxOrder = await _unitOfWork.BannerRepository.GetMaxOrderAsync();
            banner.OrderIndex = currentMaxOrder + 1;

            // Gán thông tin hệ thống
            banner.IsDeleted = false;
            banner.IsActive = true; // Mặc định active khi tạo mới
            banner.CreatedAt = DateTime.UtcNow;
            banner.CreatedBy = user;

            await _unitOfWork.BannerRepository.AddAsync(banner);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Banner banner, string user)
        {
            var existingBanner = await _unitOfWork.BannerRepository.GetByIdAsync(banner.Id);
            if (existingBanner == null || existingBanner.IsDeleted)
                throw new Exception("Banner not found");

            // Cập nhật các trường cho phép sửa
            existingBanner.Title = banner.Title;
            existingBanner.SubTitle = banner.SubTitle;
            existingBanner.TargetUrl = banner.TargetUrl;
            existingBanner.OrderIndex = banner.OrderIndex; // Cho phép đổi thứ tự

            // Nếu có gửi ảnh mới lên thì cập nhật, nếu null/empty thì giữ nguyên ảnh cũ
            if (!string.IsNullOrWhiteSpace(banner.ImageUrl))
            {
                existingBanner.ImageUrl = banner.ImageUrl;
            }

            // Gán thông tin cập nhật
            existingBanner.UpdatedAt = DateTime.UtcNow;
            existingBanner.UpdatedBy = user;

            // Lưu ý: Không update CreatedAt, CreatedBy

            await _unitOfWork.SaveAsync();
        }

        public async Task ToggleAsync(int id)
        {
            var banner = await _unitOfWork.BannerRepository.GetByIdAsync(id);
            if (banner == null || banner.IsDeleted)
                throw new Exception("Banner not found");

            // Đảo ngược trạng thái Active
            banner.IsActive = !banner.IsActive;
            banner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var banner = await _unitOfWork.BannerRepository.GetByIdAsync(id);
            if (banner == null)
                throw new Exception("Banner not found");

            banner.IsDeleted = true;
            banner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
        }
    }
}