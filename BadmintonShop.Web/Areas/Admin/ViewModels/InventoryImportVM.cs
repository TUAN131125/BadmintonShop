using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class InventoryImportVM
    {
        [Required(ErrorMessage = "Please select a product variant.")]
        public int ProductVariantId { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10,000.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Please provide a reason for import.")]
        public string Reason { get; set; }
        public decimal ImportPrice { get; set; }
    }
}