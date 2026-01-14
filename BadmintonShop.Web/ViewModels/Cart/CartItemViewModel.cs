namespace BadmintonShop.Web.ViewModels.Cart
{
    public class CartItemViewModel
    {
        public int VariantId { get; set; } // ID của biến thể (4U, 3U...)
        public int ProductId { get; set; } // ID sản phẩm cha
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}