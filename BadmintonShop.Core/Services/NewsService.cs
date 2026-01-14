using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Services
{
    public class NewsService : INewsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NewsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<News>> GetAllAsync()
        {
            // Lấy tất cả (Dùng cho Admin), sắp xếp bài mới nhất lên đầu
            var news = await _unitOfWork.NewsRepository.GetAllAsync();
            return news.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<IEnumerable<News>> GetPublishedAsync()
        {
            // Chỉ lấy bài đang Public (Dùng cho Customer)
            var news = await _unitOfWork.NewsRepository.GetAllAsync(x => x.IsPublished);
            return news.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<News> GetByIdAsync(int id)
        {
            return await _unitOfWork.NewsRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(News news)
        {
            news.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.NewsRepository.AddAsync(news);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(News news)
        {
            news.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.NewsRepository.Update(news);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var news = await _unitOfWork.NewsRepository.GetByIdAsync(id);
            if (news != null)
            {
                _unitOfWork.NewsRepository.Delete(news);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}