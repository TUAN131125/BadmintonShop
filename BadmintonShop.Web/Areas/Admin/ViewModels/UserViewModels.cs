using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    // ================= ADMIN VIEW MODEL =================
    // Dành cho quản trị viên, nhân viên (Dùng Username để đăng nhập)
    public class AdminUserVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; } // Admin có thể có Email hoặc không

        // Mật khẩu: Bắt buộc khi tạo mới, tùy chọn khi chỉnh sửa
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Role")]
        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;

        // Dropdown cho Role
        public IEnumerable<SelectListItem>? Roles { get; set; }
    }

    // ================= CUSTOMER VIEW MODEL =================
    // Dành cho khách hàng (Dùng Email, cần địa chỉ, SĐT)
    public class CustomerUserVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        // Customer dùng Email làm Username luôn
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone]
        public string Phone { get; set; }

        public string? Address { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; } = "Vietnam";

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;
    }
}