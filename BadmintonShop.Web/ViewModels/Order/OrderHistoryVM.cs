using BadmintonShop.Core.Enums;
using System;

namespace BadmintonShop.Web.ViewModels.Order
{
    public class OrderHistoryVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public int ItemCount { get; set; } // Số lượng món hàng trong đơn
    }
}