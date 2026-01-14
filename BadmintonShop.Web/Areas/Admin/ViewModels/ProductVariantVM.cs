using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class ProductVariantVM
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        // --- CORE ATTRIBUTES ---

        [Required(ErrorMessage = "SKU is required.")]
        public string SKU { get; set; }

        [Display(Name = "Attribute Name")] // e.g., Weight, Color
        [Required(ErrorMessage = "Attribute Name is required.")]
        public string AttributeName { get; set; }

        [Display(Name = "Attribute Value")] // e.g., 4U, Red
        [Required(ErrorMessage = "Attribute Value is required.")]
        public string AttributeValue { get; set; }

        // --- PRICING ---

        // Used for:
        // 1. View: Displaying 'Average Cost' from Inventory
        // 2. Create: Inputting 'Initial Import Price' for the first stock batch
        [Display(Name = "Import Price / Avg Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Import Price must be greater than or equal to 0.")]
        public decimal ImportPrice { get; set; }

        [Display(Name = "Selling Price")]
        [Required(ErrorMessage = "Selling Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Selling Price must be greater than or equal to 0.")]
        public decimal Price { get; set; }

        // --- INVENTORY ---

        // Used only for display (Read-only view of current stock)
        [Display(Name = "Current Stock")]
        public int Stock { get; set; }

        // Used only when Creating a new Variant to input initial stock
        [Display(Name = "Initial Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int Quantity { get; set; }

        // --- STATUS ---

        // [NEW] Supports Soft Delete / Business Status
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}