using BadmintonShop.Core.Entities;
using BadmintonShop.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Services.Payments
{
    public class PaypalPaymentService : IPaymentService
    {
        private readonly PaypalClient _paypalClient;
        private readonly CurrencyService _currencyService;

        public PaypalPaymentService(PaypalClient paypalClient, CurrencyService currencyService)
        {
            _paypalClient = paypalClient;
            _currencyService = currencyService;
        }

        public async Task<string> CreatePaymentUrl(Order order, string returnUrl, string cancelUrl)
        {
            // 1. Đổi tiền tệ sang chuỗi USD chuẩn (ví dụ "180.00")
            string value = _currencyService.ConvertToUsd(order.TotalAmount);

            // 2. Gọi PaypalClient mới để tạo đơn
            var response = await _paypalClient.CreateOrder(value, "USD", order.Id.ToString(), returnUrl, cancelUrl);

            // 3. Lấy link approve để redirect user
            var approvalLink = response.links.FirstOrDefault(x => x.rel == "approve");

            if (approvalLink == null)
            {
                throw new Exception("Không tìm thấy link thanh toán (approve link) từ PayPal.");
            }

            return approvalLink.href;
        }

        public async Task<bool> ExecutePayment(string paymentId, string payerId)
        {
            // paymentId chính là OrderId của PayPal
            var response = await _paypalClient.CaptureOrder(paymentId);

            // Kiểm tra trạng thái
            return response.status == "COMPLETED";
        }
    }
}