using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Enums;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class OrderController : BaseAdminController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // --- INDEX ---
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();

            // Sắp xếp mới nhất lên đầu
            var vm = orders.OrderByDescending(o => o.CreatedAt).Select(o => new OrderListVM
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            });

            return View(vm);
        }

        // --- DETAIL (ACTION CẦN SỬA) ---
        public async Task<IActionResult> Detail(int id)
        {
            // SỬA LỖI: Gọi qua Service thay vì dùng _context trực tiếp
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrderDetailVM
            {
                OrderId = order.Id,
                UserId = order.UserId,
                CreatedDate = order.CreatedAt,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Email = order.Email,

                // Gộp địa chỉ
                ShippingAddress = $"{order.ShippingAddress}, {order.Ward}, {order.City}, {order.Country}",
                Note = order.Note,

                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                IsPaid = order.IsPaid,

                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,

                Items = order.OrderDetails.Select(od => new OrderItemVM
                {
                    ProductId = od.ProductVariant?.ProductId ?? 0,
                    ProductName = od.ProductName,
                    SKU = od.SKU,

                    // Lấy thuộc tính từ Variant (check null an toàn)
                    AttributeName = od.ProductVariant?.AttributeName ?? "",
                    AttributeValue = od.ProductVariant?.AttributeValue ?? "",

                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList()
            };

            vm.SubTotal = vm.Items.Sum(x => x.Total);

            return View(vm);
        }

        // --- UPDATE STATUS ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            try
            {
                await _orderService.UpdateStatusAsync(id, status);
                TempData["Success"] = "Order status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Detail), new { id });
        }

        // --- CANCEL ORDER ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id, string reason)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int adminId);

                if (string.IsNullOrEmpty(reason)) reason = "Admin cancelled manually";

                await _orderService.CancelAsync(id, adminId, reason);
                TempData["Success"] = "Order cancelled and inventory refunded.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed: " + ex.Message;
            }
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}