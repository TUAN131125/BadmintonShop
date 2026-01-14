using BadmintonShop.Core.DTOs;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IOrderService
    {
        // --- CUSTOMER FEATURES ---

        /// <summary>
        /// Xử lý đặt hàng: Tạo đơn, trừ tồn kho
        /// </summary>
        Task<Order> CheckoutAsync(CheckoutDTO checkoutDto);

        /// <summary>
        /// Lấy danh sách lịch sử đơn hàng của một User
        /// </summary>
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);

        // --- COMMON FEATURES ---

        /// <summary>
        /// Lấy chi tiết đơn hàng (Kèm thông tin sản phẩm)
        /// </summary>
        Task<Order> GetOrderByIdAsync(int orderId);

        // --- ADMIN FEATURES ---

        /// <summary>
        /// Lấy tất cả đơn hàng (Dùng cho trang quản lý Admin)
        /// </summary>
        Task<IEnumerable<Order>> GetAllAsync();

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (Ví dụ: Pending -> Paid)
        /// </summary>
        Task UpdateStatusAsync(int orderId, OrderStatus newStatus);

        /// <summary>
        /// Hủy đơn hàng và hoàn lại số lượng tồn kho
        /// </summary>
        Task CancelAsync(int orderId, int userId, string reason);
    }
}