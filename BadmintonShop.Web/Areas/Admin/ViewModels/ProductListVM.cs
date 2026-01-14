namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class ProductListVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }
}