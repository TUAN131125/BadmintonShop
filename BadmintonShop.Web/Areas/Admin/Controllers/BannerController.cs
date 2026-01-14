using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController : BaseAdminController
    {
        private readonly IBannerService _service;
        private readonly IWebHostEnvironment _env;

        public BannerController(IBannerService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // Helper upload ảnh
        private async Task<string> SaveImage(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads/banners");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); // Kiểm tra tạo thư mục

            var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, name);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/banners/{name}";
        }

        public async Task<IActionResult> Index()
        {
            var model = await _service.GetAllAsync();
            return View(model);
        }

        // GET: Create
        public IActionResult Create()
        {
            // Truyền Entity rỗng để View binding
            return View(new Banner());
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(Banner banner, IFormFile? imageFile)
        {
            // Validate: Bắt buộc phải có ảnh khi tạo mới
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "Please upload an image");
            }

            // Validate các trường khác của Entity
            if (string.IsNullOrEmpty(banner.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }

            if (ModelState.IsValid)
            {
                // Upload ảnh và gán đường dẫn vào Entity
                if (imageFile != null)
                {
                    banner.ImageUrl = await SaveImage(imageFile);
                }

                // Gọi Service với Entity trực tiếp
                await _service.CreateAsync(banner, User.Identity?.Name ?? "system");

                return RedirectToAction(nameof(Index));
            }

            return View(banner);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _service.GetByIdAsync(id);
            if (banner == null) return NotFound();

            // Truyền trực tiếp Entity xuống View
            return View(banner);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Banner banner, IFormFile? imageFile)
        {
            if (string.IsNullOrEmpty(banner.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }

            if (ModelState.IsValid)
            {
                // Nếu có chọn ảnh mới thì upload và gán lại ImageUrl
                if (imageFile != null && imageFile.Length > 0)
                {
                    banner.ImageUrl = await SaveImage(imageFile);
                }
                // Nếu không chọn ảnh mới (imageFile == null), Service sẽ giữ nguyên ảnh cũ 
                // dựa trên logic trong BannerService.UpdateAsync hoặc giá trị từ hidden field

                await _service.UpdateAsync(banner, User.Identity?.Name ?? "system");

                return RedirectToAction(nameof(Index));
            }

            return View(banner);
        }

        public async Task<IActionResult> Toggle(int id)
        {
            await _service.ToggleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _service.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}