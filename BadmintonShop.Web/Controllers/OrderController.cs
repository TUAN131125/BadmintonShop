using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BadmintonShop.Core.Enums;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Trang danh sách (History)
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var orders = await _orderService.GetUserOrdersAsync(userId);

            // --- MAPPING ENTITY -> VIEWMODEL ---
            var viewModels = orders.Select(x => new OrderHistoryVM
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                ItemCount = x.OrderDetails?.Count ?? 0
            }).ToList();

            return View(viewModels);
        }

        // 2. Trang chi tiết (Detail)
        public async Task<IActionResult> Detail(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null) return NotFound();

            // Bảo mật: Không cho xem đơn của người khác
            if (order.UserId != userId) return Forbid();

            // --- MAPPING ENTITY -> VIEWMODEL ---
            var viewModel = new OrderDetailVM
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                Note = order.Note,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Email = order.Email,
                FullAddress = $"{order.ShippingAddress}, {order.Ward}, {order.City}",
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                Items = order.OrderDetails.Select(d => new OrderItemVM
                {
                    ProductName = d.ProductName,
                    SKU = d.SKU,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            // 1. Lấy User ID hiện tại
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            try
            {
                // 2. Lấy đơn hàng để kiểm tra
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null) return NotFound();

                // [BẢO MẬT] Chỉ chủ sở hữu mới được hủy
                if (order.UserId != userId)
                {
                    return Forbid();
                }

                // [LOGIC] Chỉ cho phép hủy khi đơn còn mới (Pending hoặc Chờ thanh toán)
                // Nếu Admin đã xác nhận (Processing) hoặc đang giao (Shipping) thì chặn lại.
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.AwaitingPayment)
                {
                    // "Cannot cancel this order as it has already been processed or is being shipped."
                    TempData["Error"] = "Cannot cancel this order as it has already been processed or is being shipped.";
                    return RedirectToAction("Detail", new { id = id });
                }

                // 3. Gọi Service Hủy (Lý do: "Customer requested cancellation")
                await _orderService.CancelAsync(id, userId, "Customer requested cancellation.");

                // "Order cancelled successfully. A refund will be issued if payment was made."
                TempData["SuccessMessage"] = "Order cancelled successfully. A refund will be issued if payment was made.";
            }
            catch (Exception ex)
            {
                // "Error cancelling order: ..."
                TempData["Error"] = "Error cancelling order: " + ex.Message;
            }

            // Quay lại trang chi tiết để xem trạng thái mới
            return RedirectToAction("Detail", new { id = id });
        }
    }
}