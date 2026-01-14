using System.ComponentModel.DataAnnotations;
using BadmintonShop.Web.ViewModels.Cart;

namespace BadmintonShop.Web.ViewModels
{
    public class CheckoutVM
    {
        // --- Recipient Information ---
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string Phone { get; set; }

        // --- New Address Structure (No District) ---
        [Required(ErrorMessage = "City/Province is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Ward/Commune is required.")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Specific Address (House No, Street) is required.")]
        public string SpecificAddress { get; set; }

        public string? Note { get; set; }

        // --- Payment ---
        [Required]
        public string PaymentMethod { get; set; } = "COD";

        // --- Display Data (Read-only for View) ---
        public List<CartItemViewModel>? CartItems { get; set; } // Allow null to avoid validation error
        public decimal GrandTotal { get; set; }
    }
}