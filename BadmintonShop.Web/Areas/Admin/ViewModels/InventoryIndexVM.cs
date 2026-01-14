namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class InventoryIndexVM
    {
        public int Id { get; set; } // ProductVariantId
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string AttributeName { get; set; }   // VD: Weight
        public string AttributeValue { get; set; }  // VD: 4U
        public int Quantity { get; set; }
        public decimal AverageCost { get; set; } // [MỚI] Giá vốn
        public bool IsLowStock { get; set; }
    }
}