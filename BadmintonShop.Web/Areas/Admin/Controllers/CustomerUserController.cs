using AutoMapper;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerUserController : BaseAdminController
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public CustomerUserController(IUserService userService, IRoleService roleService, IMapper mapper)
        {
            _userService = userService;
            _roleService = roleService;
            _mapper = mapper;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetCustomersAsync();
            // Map Entity -> CustomerUserVM (Tự động hiển thị Email, Phone...)
            var vm = _mapper.Map<IEnumerable<CustomerUserVM>>(users);
            return View("~/Areas/Admin/Views/User/Customer/Index.cshtml", vm);
        }

        // CREATE
        public IActionResult Create()
        {
            return View("~/Areas/Admin/Views/User/Customer/Create.cshtml", new CustomerUserVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerUserVM vm)
        {
            if (string.IsNullOrEmpty(vm.Password))
                ModelState.AddModelError("Password", "Password is required");

            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/User/Customer/Create.cshtml", vm);

            // Lấy Role Customer
            var customerRole = await _roleService.GetByNameAsync("Customer");
            if (customerRole == null) return BadRequest("Role 'Customer' not found in DB");

            // Map VM -> Entity
            var user = _mapper.Map<User>(vm);
            user.RoleId = customerRole.Id;
            user.Username = vm.Email; // Quy ước: Customer dùng Email làm Username

            await _userService.CreateAsync(user, vm.Password);

            TempData["Success"] = "Customer created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            var vm = _mapper.Map<CustomerUserVM>(user);
            return View("~/Areas/Admin/Views/User/Customer/Edit.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerUserVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/User/Customer/Edit.cshtml", vm);

            var user = await _userService.GetByIdAsync(vm.Id);
            if (user == null) return NotFound();

            // Cập nhật thông tin (Email, Phone, Address...)
            _mapper.Map(vm, user);
            user.Username = vm.Email; // Đảm bảo Username luôn đồng bộ Email

            // Xử lý đổi mật khẩu nếu có nhập
            if (!string.IsNullOrEmpty(vm.Password))
            {
                user.PasswordHash = HashPasswordHelper(vm.Password);
            }

            await _userService.UpdateAsync(user);
            TempData["Success"] = "Customer updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper Hash Password
        private string HashPasswordHelper(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }

        // Lock/Unlock giữ nguyên...
        [HttpPost]
        public async Task<IActionResult> Lock(int id) { await _userService.LockAsync(id); return RedirectToAction(nameof(Index)); }

        [HttpPost]
        public async Task<IActionResult> Unlock(int id) { await _userService.UnlockAsync(id); return RedirectToAction(nameof(Index)); }
    }
}