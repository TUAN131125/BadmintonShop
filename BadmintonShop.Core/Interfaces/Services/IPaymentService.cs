using BadmintonShop.Core.Entities; // Đảm bảo import Entity Order
using System.Threading.Tasks;

namespace BadmintonShop.Web.Services.Payments
{
    public interface IPaymentService
    {
        // Hàm tạo URL thanh toán
        Task<string> CreatePaymentUrl(Order order, string returnUrl, string cancelUrl);

        // Hàm xác nhận thanh toán
        Task<bool> ExecutePayment(string paymentId, string payerId);
    }
}