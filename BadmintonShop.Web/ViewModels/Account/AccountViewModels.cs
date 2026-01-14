using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        // Thêm trường Tỉnh/Thành phố
        [Required(ErrorMessage = "Please select your city/province")]
        public string Province { get; set; }

        // Trường này dành cho: Số nhà, Đường, Phường/Xã, Quận/Huyện
        [Required(ErrorMessage = "Specific address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginVM
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
