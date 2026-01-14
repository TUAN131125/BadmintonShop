using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Data.DbContext;
using BadmintonShop.Web.Areas.Admin.ViewModels; // Namespace chứa ProductVM và ProductVariantVM
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : BaseAdminController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductVariantService _variantService;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IProductVariantService variantService, // Inject thêm service này
            IWebHostEnvironment env,
            ApplicationDbContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _variantService = variantService;
            _env = env;
            _context = context;
        }

        // --- 1. INDEX (DANH SÁCH) ---
        public async Task<IActionResult> Index()
        {
            var entities = await _productService.GetAllAsync(includeInactive: true);

            var model = entities.Select(p => new ProductListVM
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                IsActive = p.IsActive,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Categories != null ? string.Join(", ", p.Categories.Select(c => c.Name)) : ""
            }).ToList();

            return View(model);
        }

        // --- 2. DETAIL (XEM CHI TIẾT) ---
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            // 1. Truy vấn Product kèm Category
            // Dùng _context trực tiếp hoặc qua Service đều được. 
            // Ở đây tôi dùng _context để tiện Include sâu nếu cần.
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // 2. Truy vấn Variants kèm Inventory (Quan trọng: Phải có Inventory để lấy Giá vốn)
            var variants = await _context.ProductVariants
                .Include(v => v.Inventory)
                .Where(v => v.ProductId == id)
                .ToListAsync();

            // 3. Map sang ViewModel
            var vm = new ProductDetailAdminVM
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Description = product.Description,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CategoryName = product.Categories.Any()
                               ? string.Join(", ", product.Categories.Select(c => c.Name))
                               : "Chưa phân loại",

                // Thống kê kho
                TotalVariants = variants.Count,
                TotalStock = variants.Sum(v => v.Inventory?.Quantity ?? 0),

                // Giá trị vốn kho = Tổng (Số lượng * Giá vốn)
                TotalStockValue = variants.Sum(v => (v.Inventory?.Quantity ?? 0) * (v.Inventory?.AverageCost ?? 0)),

                // Danh sách chi tiết
                Variants = variants.Select(v => new VariantDetailVM
                {
                    SKU = v.SKU,
                    AttributeName = v.AttributeName,
                    AttributeValue = v.AttributeValue,
                    SalePrice = v.Price,
                    ImportPrice = v.Inventory?.AverageCost ?? 0, // Giá vốn từ bảng Inventory
                    Stock = v.Inventory?.Quantity ?? 0
                }).ToList()
            };

            return View(vm);
        }

        // --- 3. CREATE (TẠO MỚI) ---
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            // Sử dụng ProductVM (class có sẵn) thay vì ProductCreateVM
            return View(new ProductVM());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductVM vm)
        {
            if (ModelState.IsValid)
            {
                string? imagePath = null;
                if (vm.ImageFile != null)
                {
                    imagePath = await UploadFile(vm.ImageFile);
                }

                var productEntity = new Product
                {
                    Name = vm.Name,
                    // Bỏ dòng Code = vm.Code vì Entity không có thuộc tính này
                    Brand = vm.Brand ?? "",
                    BasePrice = vm.BasePrice,
                    SalePrice = vm.SalePrice,
                    Description = vm.Description ?? "",
                    ImageUrl = imagePath,
                    IsActive = vm.IsActive
                };

                await _productService.CreateProductAsync(productEntity, vm.CategoryIds);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(vm);
        }

        // --- 4. EDIT (CẬP NHẬT) ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _productService.GetByIdAsync(id);
            if (p == null) return NotFound();

            // Sử dụng ProductVM cho cả Edit
            var vm = new ProductVM
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                Description = p.Description,
                IsActive = p.IsActive,
                ImageUrl = p.ImageUrl,
                CategoryIds = p.Categories.Select(c => c.Id).ToList()
            };

            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductVM vm)
        {
            if (ModelState.IsValid)
            {
                string? newImagePath = null;
                if (vm.ImageFile != null)
                {
                    newImagePath = await UploadFile(vm.ImageFile);
                }

                var productEntity = new Product
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Brand = vm.Brand ?? "",
                    BasePrice = vm.BasePrice,
                    SalePrice = vm.SalePrice,
                    Description = vm.Description ?? "",
                    IsActive = vm.IsActive,
                    // Giữ ảnh cũ nếu không upload ảnh mới
                    ImageUrl = newImagePath
                };

                await _productService.UpdateProductAsync(productEntity, vm.CategoryIds);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(vm);
        }

        // --- 5. DELETE (XÓA) ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- HELPER UPLOAD ---
        private async Task<string> UploadFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string folderName = "products";
            string wwwRootPath = Path.Combine(_env.WebRootPath, "images", folderName);

            if (!Directory.Exists(wwwRootPath))
                Directory.CreateDirectory(wwwRootPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(wwwRootPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/images/{folderName}/{fileName}";
        }
    }
}