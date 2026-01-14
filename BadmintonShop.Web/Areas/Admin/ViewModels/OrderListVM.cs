using BadmintonShop.Core.Enums;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class OrderListVM
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
