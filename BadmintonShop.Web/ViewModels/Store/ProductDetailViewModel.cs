using BadmintonShop.Web.Areas.Admin.ViewModels;

namespace BadmintonShop.Web.ViewModels.Store
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }
        public string Brand { get; set;}

        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }

        public decimal CurrentPrice { get; set; }
        public bool IsOnSale { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal DisplayPrice { get; set; }
        public IEnumerable<ProductItemViewModel> RelatedProducts { get; set; }
        public List<ProductVariantVM> Variants { get; set; } = new List<ProductVariantVM>();
    }

    public class ProductVariantVM
    {
        public int Id { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        public decimal OriginalPrice { get; set; } // Giá gốc của variant
        public decimal CurrentPrice { get; set; } // [QUAN TRỌNG] Giá bán thực tế của variant

        public int Stock { get; set; }
    }
}