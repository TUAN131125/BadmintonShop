using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : BaseAdminController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // LIST (TREE)
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetCategoryTreeAsync();

            var vm = FlattenCategoryTree(categories, null);
            return View(vm);
        }

        // CREATE
        public async Task<IActionResult> Create()
        {
            ViewBag.Parents = await _categoryService.GetRootCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Parents = await _categoryService.GetRootCategoriesAsync();
                return View(model);
            }

            await _categoryService.CreateAsync(new Category
            {
                Name = model.Name,
                ParentId = model.ParentId
            });

            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            var vm = new CategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId
            };

            ViewBag.Parents = await _categoryService.GetRootCategoriesAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Parents = await _categoryService.GetRootCategoriesAsync();
                return View(model);
            }

            await _categoryService.UpdateAsync(new Category
            {
                Id = model.Id,
                Name = model.Name,
                ParentId = model.ParentId
            });

            return RedirectToAction(nameof(Index));
        }

        // DELETE (SOFT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // PRIVATE – TREE FLATTEN
        // ===============================
        private List<CategoryVM> FlattenCategoryTree(
            IEnumerable<Category> categories,
            string prefix)
        {
            var result = new List<CategoryVM>();

            foreach (var category in categories)
            {
                result.Add(new CategoryVM
                {
                    Id = category.Id,
                    Name = prefix == null
                        ? category.Name
                        : $"{prefix} ─ {category.Name}",
                    ParentId = category.ParentId
                });

                if (category.Children != null && category.Children.Any())
                {
                    result.AddRange(
                        FlattenCategoryTree(
                            category.Children,
                            prefix == null
                                ? category.Name
                                : $"{prefix} ─ {category.Name}"
                        )
                    );
                }
            }

            return result;
        }
    }
}
