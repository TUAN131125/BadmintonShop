using System.Collections.Generic;

namespace BadmintonShop.Web.ViewModels.Store
{
    public class ProductListViewModel
    {
        public List<ProductItemViewModel> Products { get; set; } = new();

        // Filter values coming from UI
        public string Brand { get; set; }
        public string PriceRange { get; set; }
        public string PlayStyle { get; set; }
        public string Level { get; set; }
        public List<string>? Brands { get; set; }
        public List<string>? Prices { get; set; }
    }
}
