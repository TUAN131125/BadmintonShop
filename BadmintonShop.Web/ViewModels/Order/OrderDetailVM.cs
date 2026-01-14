using BadmintonShop.Core.Enums;
using System;
using System.Collections.Generic;

namespace BadmintonShop.Web.ViewModels.Order
{
    public class OrderDetailVM
    {
        // Thông tin chung
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public string Note { get; set; }

        // Thông tin giao hàng
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string FullAddress { get; set; } // Gộp Address, Ward, City

        // Thông tin tiền
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }

        // Danh sách sản phẩm
        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();
    }

    public class OrderItemVM
    {
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice; // Tự tính toán
    }
}