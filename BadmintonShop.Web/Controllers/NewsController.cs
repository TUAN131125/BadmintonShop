using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.ViewModels.News; // Nhớ using namespace này
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        // 1. Danh sách tin tức
        public async Task<IActionResult> Index()
        {
            var allNews = await _newsService.GetAllAsync();

            // Lọc & Map sang ViewModel
            var viewModels = allNews
                .Where(x => x.IsPublished) // Chỉ lấy bài đã đăng
                .OrderByDescending(x => x.CreatedAt) // Mới nhất lên đầu
                .Select(x => new NewsItemVM
                {
                    Id = x.Id,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return View(viewModels);
        }

        // 2. Chi tiết bài viết
        public async Task<IActionResult> Detail(int id)
        {
            var news = await _newsService.GetByIdAsync(id);

            if (news == null || !news.IsPublished)
            {
                return NotFound();
            }

            // Map sang ViewModel chi tiết
            var viewModel = new NewsDetailVM
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                ImageUrl = news.ImageUrl,
                CreatedAt = news.CreatedAt,
                Author = "Badminton Shop Team" // Hoặc lấy tên người tạo
            };

            return View(viewModel);
        }
    }
}