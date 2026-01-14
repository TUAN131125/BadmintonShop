using BadmintonShop.Core.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này

namespace BadmintonShop.Core.Entities
{
    public class Order : BaseEntity
    {
        public int? UserId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ShippingAddress { get; set; }

        public string Country { get; set; }
        public string City { get; set; }
        public string Ward { get; set; }
        // public string District { get; set; } // Đã bỏ theo hướng dẫn trước

        public string PaymentMethod { get; set; }
        public decimal ShippingFee { get; set; }

        // ---------------------------------------------------------
        // SỬA LỖI TẠI ĐÂY
        // ---------------------------------------------------------
        public decimal TotalAmount { get; set; } // Tên chuẩn trong database

        public bool IsPaid { get; set; } // <--- THÊM MỚI: Để sửa lỗi 'IsPaid'
        // ---------------------------------------------------------

        public string? Note { get; set; }
        public OrderStatus Status { get; set; }
        public ICollection<OrderHistory> OrderHistories { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}