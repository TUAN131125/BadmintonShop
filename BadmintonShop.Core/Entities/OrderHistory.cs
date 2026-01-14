using BadmintonShop.Core.Enums;

namespace BadmintonShop.Core.Entities
{
    public class OrderHistory : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public OrderStatus Status { get; set; } // Trạng thái tại thời điểm đó

        public string? Note { get; set; } // Ghi chú (VD: "Khách hẹn giao chiều")

        // Người thực hiện thay đổi trạng thái (Admin/Staff)
        public int? ModifiedByUserId { get; set; }
        public User? ModifiedByUser { get; set; }
    }
}