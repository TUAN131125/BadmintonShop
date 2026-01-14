using AutoMapper;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminUserController : BaseAdminController
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper; // Dùng Mapper

        public AdminUserController(IUserService userService, IRoleService roleService, IMapper mapper)
        {
            _userService = userService;
            _roleService = roleService;
            _mapper = mapper;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAdminsAsync();
            // Map List<User> -> List<AdminUserVM>
            var vm = _mapper.Map<IEnumerable<AdminUserVM>>(users);
            return View("~/Areas/Admin/Views/User/Admin/Index.cshtml", vm);
        }

        // CREATE
        public async Task<IActionResult> Create()
        {
            var vm = new AdminUserVM();
            await LoadRoles(vm);
            return View("~/Areas/Admin/Views/User/Admin/Create.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserVM vm)
        {
            if (string.IsNullOrEmpty(vm.Password))
                ModelState.AddModelError("Password", "Password is required for new users");

            if (!ModelState.IsValid)
            {
                await LoadRoles(vm);
                return View("~/Areas/Admin/Views/User/Admin/Create.cshtml", vm);
            }

            // Map VM -> Entity
            var user = _mapper.Map<User>(vm);

            // UserService sẽ lo việc Hash Password
            await _userService.CreateAsync(user, vm.Password);

            TempData["Success"] = "Admin user created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            // Map Entity -> VM
            var vm = _mapper.Map<AdminUserVM>(user);
            await LoadRoles(vm);

            return View("~/Areas/Admin/Views/User/Admin/Edit.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadRoles(vm);
                return View("~/Areas/Admin/Views/User/Admin/Edit.cshtml", vm);
            }

            var user = await _userService.GetByIdAsync(vm.Id);
            if (user == null) return NotFound();

            // AutoMapper cập nhật các trường từ VM vào User Entity (trừ Password)
            _mapper.Map(vm, user);

            // Nếu người dùng nhập mật khẩu mới thì cập nhật
            if (!string.IsNullOrEmpty(vm.Password))
            {
                // Gọi hàm CreateAsync để tận dụng logic HashPass hoặc phải viết thêm logic Hash ở đây
                // Tốt nhất là thêm hàm UpdatePassword trong UserService. 
                // Ở đây tôi dùng cách thủ công để tránh sửa Service quá nhiều:
                user.PasswordHash = HashPasswordHelper(vm.Password);
            }

            await _userService.UpdateAsync(user);
            TempData["Success"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper Hash Password (tạm thời để ở đây hoặc move vào Service)
        private string HashPasswordHelper(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }

        private async Task LoadRoles(AdminUserVM vm)
        {
            var roles = await _roleService.GetAllAsync();
            vm.Roles = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
        }

        // Lock/Unlock giữ nguyên...
        [HttpPost]
        public async Task<IActionResult> Lock(int id) { await _userService.LockAsync(id); return RedirectToAction(nameof(Index)); }

        [HttpPost]
        public async Task<IActionResult> Unlock(int id) { await _userService.UnlockAsync(id); return RedirectToAction(nameof(Index)); }
    }
}