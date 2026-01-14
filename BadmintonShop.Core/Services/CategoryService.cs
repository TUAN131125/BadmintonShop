using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Helpers;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore; // Cần thiết cho Include, ToListAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. Lấy toàn bộ cây category (menu)
        public async Task<IEnumerable<Category>> GetCategoryTreeAsync()
        {
            return await _unitOfWork.CategoryRepository.GetRootCategoriesAsync();
        }

        // 2. Lấy category gốc
        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _unitOfWork.CategoryRepository.GetRootCategoriesAsync();
        }

        // 3. Lấy category con
        public async Task<IEnumerable<Category>> GetChildrenAsync(int parentId)
        {
            return await _unitOfWork.CategoryRepository.GetChildrenAsync(parentId);
        }

        // 4. Lấy sản phẩm theo category (bao gồm category con)
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null || category.IsDeleted)
                throw new Exception("Category not found");

            var categoryIds = new List<int>();
            await CollectCategoryIds(categoryId, categoryIds);

            // --- SỬA LỖI LOGIC TẠI ĐÂY ---
            // Cũ: p.CategoryId (sai vì đã xóa cột này)
            // Mới: Kiểm tra xem danh sách Categories của sản phẩm có chứa ID cần tìm không

            return await _unitOfWork.ProductRepository.GetQuery()
                .Include(p => p.Categories) // Load bảng liên kết
                .Where(p => !p.IsDeleted && p.IsActive
                            && p.Categories.Any(c => categoryIds.Contains(c.Id))) // Logic lọc Nhiều - Nhiều
                .ToListAsync();
        }

        // ===============================
        // PRIVATE – RECURSIVE TREE LOGIC
        // ===============================
        private async Task CollectCategoryIds(int categoryId, List<int> result)
        {
            result.Add(categoryId);

            var children = await _unitOfWork.CategoryRepository.GetChildrenAsync(categoryId);
            foreach (var child in children)
            {
                await CollectCategoryIds(child.Id, result);
            }
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
                return null;

            return category;
        }

        public async Task CreateAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new Exception("Category name is required");

            category.Slug = SlugHelper.GenerateSlug(category.Name);
            category.CreatedAt = DateTime.UtcNow;
            category.IsDeleted = false;

            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            var existing = await _unitOfWork.CategoryRepository.GetByIdAsync(category.Id);
            if (existing == null)
                throw new Exception("Category not found");

            existing.Name = category.Name;
            existing.ParentId = category.ParentId;
            existing.Slug = SlugHelper.GenerateSlug(category.Name);
            existing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Category not found");

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _unitOfWork.CategoryRepository.GetAllAsync(x => !x.IsDeleted);
        }
    }
}