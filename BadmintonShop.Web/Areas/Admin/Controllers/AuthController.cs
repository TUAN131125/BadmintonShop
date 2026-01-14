using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Kiểm tra xem đã có vé "AdminAuth" chưa
            if (User.Identity!.IsAuthenticated && User.Identity.AuthenticationType == "AdminAuth")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _auth.AuthenticateAsync(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // QUAN TRỌNG: Identity phải dùng scheme "AdminAuth"
            var identity = new ClaimsIdentity(claims, "AdminAuth");
            var principal = new ClaimsPrincipal(identity);

            // Sign In vào Scheme "AdminAuth" (Cookie Admin)
            await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTime.UtcNow.AddHours(12)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign Out khỏi Scheme "AdminAuth"
            await HttpContext.SignOutAsync("AdminAuth");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}