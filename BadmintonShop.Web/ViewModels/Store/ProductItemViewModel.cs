namespace BadmintonShop.Web.ViewModels.Store
{
    public class ProductItemViewModel
    {
        public int Id { get; set; }          // int đúng với entity
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? ImageUrl { get; set; }
        public string Brand { get; set; }

        public decimal CurrentPrice { get; set; }
        public bool IsOnSale { get; set; }
        public int DiscountPercent { get; set; }

        public decimal DisplayPrice { get; set; }   // computed price
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
    }
}
