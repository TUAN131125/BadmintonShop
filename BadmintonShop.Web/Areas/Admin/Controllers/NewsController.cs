using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.ViewModels.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : BaseAdminController
    {
        private readonly INewsService _newsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(INewsService newsService, IWebHostEnvironment webHostEnvironment)
        {
            _newsService = newsService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ================== 1. DANH SÁCH ==================
        public async Task<IActionResult> Index()
        {
            var newsList = await _newsService.GetAllAsync();
            return View(newsList);
        }

        // ================== 2. TẠO MỚI ==================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new NewsCreateUpdateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsCreateUpdateVM model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = "/images/default-news.jpg"; // Ảnh mặc định

                // Xử lý upload ảnh
                if (model.ImageFile != null)
                {
                    imagePath = await SaveImage(model.ImageFile);
                }

                var news = new News
                {
                    Title = model.Title,
                    Slug = GenerateSlug(model.Title),
                    ShortDescription = model.ShortDescription,
                    Content = model.Content,
                    IsPublished = model.IsPublished,
                    ImageUrl = imagePath,
                    CreatedAt = DateTime.UtcNow
                };

                await _newsService.CreateAsync(news);
                TempData["Success"] = "Thêm tin tức thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ================== 3. CHỈNH SỬA ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            if (news == null) return NotFound();

            // Map Entity -> ViewModel để hiển thị lên form
            var vm = new NewsCreateUpdateVM
            {
                Id = news.Id,
                Title = news.Title,
                ShortDescription = news.ShortDescription,
                Content = news.Content,
                IsPublished = news.IsPublished,
                ExistingImageUrl = news.ImageUrl // Lưu ảnh cũ để hiển thị
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewsCreateUpdateVM model)
        {
            if (ModelState.IsValid)
            {
                var news = await _newsService.GetByIdAsync(model.Id);
                if (news == null) return NotFound();

                // Cập nhật thông tin cơ bản
                news.Title = model.Title;
                news.Slug = GenerateSlug(model.Title);
                news.ShortDescription = model.ShortDescription;
                news.Content = model.Content;
                news.IsPublished = model.IsPublished;

                // Xử lý ảnh: Chỉ thay đổi nếu người dùng upload ảnh mới
                if (model.ImageFile != null)
                {
                    // (Tùy chọn) Xóa ảnh cũ nếu không phải ảnh mặc định
                    if (!string.IsNullOrEmpty(news.ImageUrl) && !news.ImageUrl.Contains("default"))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, news.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Lưu ảnh mới
                    news.ImageUrl = await SaveImage(model.ImageFile);
                }

                await _newsService.UpdateAsync(news);
                TempData["Success"] = "Cập nhật tin tức thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ================== 4. XÓA ==================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            if (news == null) return NotFound();

            // Xóa ảnh vật lý (nếu cần)
            if (!string.IsNullOrEmpty(news.ImageUrl) && !news.ImageUrl.Contains("default"))
            {
                string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, news.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            // Gọi Service xóa (Hàm này bạn vừa sửa ở bước trước)
            await _newsService.DeleteAsync(id);

            TempData["Success"] = "Đã xóa tin tức.";
            return RedirectToAction(nameof(Index));
        }

        // --- HELPER METHODS ---
        private async Task<string> SaveImage(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/news");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/news/" + uniqueFileName;
        }

        private string GenerateSlug(string title)
        {
            // Hàm đơn giản, bạn có thể dùng thư viện Slugify để tốt hơn
            return title.ToLower()
                .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace(" ", "-").Replace(",", "").Replace(".", "").Replace("?", "");
        }
    }
}