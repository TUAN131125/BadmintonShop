using BadmintonShop.Core.DTOs;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Enums;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Helpers;
using BadmintonShop.Web.Services.Payments;
using BadmintonShop.Web.ViewModels;
using BadmintonShop.Web.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BadmintonShop.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public CheckoutController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var cartVM = cart.Select(c => new CartItemViewModel
            {
                ProductId = c.ProductId,
                VariantId = c.VariantId,
                ProductName = c.ProductName,
                Price = c.Price,
                Quantity = c.Quantity,
                ImageUrl = c.ImageUrl
            }).ToList();

            var userPhone = User.FindFirstValue(ClaimTypes.MobilePhone);
            var userName = User.Identity.Name;

            var model = new CheckoutVM
            {
                CartItems = cartVM,
                GrandTotal = cartVM.Sum(x => x.Total),
                FullName = userName,
                Phone = userPhone ?? "",
                City = "",
                Ward = "",
                SpecificAddress = "",
                PaymentMethod = "COD" // Mặc định
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            // 1. Lấy giỏ hàng
            var cart = CartSessionHelper.GetCart(HttpContext);
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            // 2. Map dữ liệu để hiển thị lại nếu lỗi validation
            var cartVM = cart.Select(c => new CartItemViewModel
            {
                ProductId = c.ProductId,
                VariantId = c.VariantId,
                ProductName = c.ProductName,
                Price = c.Price,
                Quantity = c.Quantity,
                ImageUrl = c.ImageUrl
            }).ToList();

            model.CartItems = cartVM;
            model.GrandTotal = cartVM.Sum(x => x.Total);
            ModelState.Remove("CartItems");
            ModelState.Remove("GrandTotal");

            if (!ModelState.IsValid) return View("Index", model);

            // ==================================================================
            // QUAN TRỌNG: ĐÃ VÔ HIỆU HÓA TRY-CATCH ĐỂ HIỆN LỖI CHI TIẾT
            // ==================================================================
            // try 
            // { 
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = string.IsNullOrEmpty(userIdString) ? null : int.Parse(userIdString);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // 3. Tạo DTO
            var checkoutDto = new CheckoutDTO
            {
                UserId = userId,
                FullName = model.FullName,
                Email = userEmail ?? "no-email@example.com",
                Phone = model.Phone,
                ShippingAddress = $"{model.SpecificAddress}, {model.Ward}, {model.City}" +
                                  (!string.IsNullOrEmpty(model.Note) ? $" (Note: {model.Note})" : ""),
                City = model.City,
                Ward = model.Ward,
                PaymentMethod = model.PaymentMethod,
                ShippingFee = 0,
                Items = cart.Select(c => new CheckoutItemDTO
                {
                    ProductVariantId = c.VariantId,
                    Quantity = c.Quantity
                }).ToList()
            };

            // 4. Gọi Service tạo đơn hàng
            var order = await _orderService.CheckoutAsync(checkoutDto);

            // 5. Xử lý thanh toán PayPal
            if (model.PaymentMethod == "PAYPAL")
            {
                var returnUrl = Url.Action("PaymentCallback", "Checkout", new { orderId = order.Id }, Request.Scheme);
                var cancelUrl = Url.Action("PaymentCallback", "Checkout", new { orderId = order.Id, error = "cancelled" }, Request.Scheme);

                // NẾU CÓ LỖI, NÓ SẼ DỪNG TẠI ĐÂY VÀ HIỆN MÀN HÌNH JSON CHI TIẾT
                var approvalUrl = await _paymentService.CreatePaymentUrl(order, returnUrl, cancelUrl);

                CartSessionHelper.ClearCart(HttpContext);
                return Redirect(approvalUrl);
            }

            // Xử lý COD
            CartSessionHelper.ClearCart(HttpContext);
            return RedirectToAction("Success", new { orderId = order.Id });

            // } 
            // catch (Exception ex)
            // {
            //     // Đã vô hiệu hóa phần giấu lỗi này
            //     var errorMessage = ex.Message;
            //     if (ex.InnerException != null) errorMessage += " " + ex.InnerException.Message;
            //     TempData["Error"] = "Lỗi đặt hàng: " + errorMessage;
            //     return RedirectToAction("Index", "Cart");
            // }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(int orderId, string token, string PayerID, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
                int userId = (claimId != null) ? int.Parse(claimId.Value) : 0;

                await _orderService.CancelAsync(orderId, userId, "Hệ thống hủy tự động: Thanh toán PayPal thất bại.");

                TempData["Error"] = "Bạn đã hủy thanh toán PayPal. Đơn hàng đã bị hủy.";
                return RedirectToAction("Index", "Cart");
            }

            var result = await _paymentService.ExecutePayment(token, PayerID);

            if (result)
            {
                await _orderService.UpdateStatusAsync(orderId, OrderStatus.Paid);
                return RedirectToAction("Success", new { orderId = orderId });
            }
            else
            {
                TempData["Error"] = "Thanh toán thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        public IActionResult Success(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}