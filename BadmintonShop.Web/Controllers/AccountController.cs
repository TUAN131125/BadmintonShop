using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BadmintonShop.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated && User.Identity.AuthenticationType == "CustomerAuth")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _userService.RegisterCustomerAsync(
                    model.FullName, model.Email, model.Phone,
                    model.Address, model.Province, model.Password
                );
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated && User.Identity.AuthenticationType == "CustomerAuth")
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userService.AuthenticateAsync(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? "User"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "Customer")
            };

            // QUAN TRỌNG: Identity phải dùng scheme "CustomerAuth"
            var identity = new ClaimsIdentity(claims, "CustomerAuth");
            var principal = new ClaimsPrincipal(identity);

            // Sign In vào Scheme "CustomerAuth" (Cookie Khách)
            await HttpContext.SignInAsync("CustomerAuth", principal, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            // Sign Out khỏi Scheme "CustomerAuth"
            await HttpContext.SignOutAsync("CustomerAuth");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}