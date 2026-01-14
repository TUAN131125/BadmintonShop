using BadmintonShop.Core.Enums;
using System;
using System.Collections.Generic;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class OrderDetailVM
    {
        public int OrderId { get; set; }
        public int? UserId { get; set; }

        // --- CÁC BIẾN VIEW CẦN (Khớp với Order.cs) ---
        public DateTime CreatedDate { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // View chỉ cần 1 dòng địa chỉ, nhưng Entity lưu rời -> Ta sẽ gộp ở Controller
        public string ShippingAddress { get; set; }
        public string Note { get; set; }

        // --- TIỀN TỆ ---
        public decimal TotalAmount { get; set; } // Tổng bill
        public decimal ShippingFee { get; set; } // Phí ship
        public decimal SubTotal { get; set; }    // Tiền hàng

        // --- TRẠNG THÁI ---
        public OrderStatus Status { get; set; }

        // [QUAN TRỌNG] Sửa đúng theo Entity Order.cs bạn gửi:
        public string PaymentMethod { get; set; } // Entity là string
        public bool IsPaid { get; set; }          // Entity là bool

        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();
    }

    public class OrderItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }

        // [QUAN TRỌNG] Thêm 2 dòng này để View hiển thị (Size/Màu)
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Tính tổng tiền cho dòng này
        public decimal Total => Quantity * UnitPrice;
    }
}