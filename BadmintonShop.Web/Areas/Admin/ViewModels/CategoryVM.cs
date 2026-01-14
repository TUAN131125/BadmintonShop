using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class CategoryVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Category name is required")]
        public string Name { get; set; }

        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
    }
}
