using System;
using System.Globalization;

namespace BadmintonShop.Web.Services
{
    public class CurrencyService
    {
        // Tỷ giá: 1 USD = 25,000 VND
        private const decimal ExchangeRate = 25000m;

        public string ConvertToUsd(decimal vndAmount)
        {
            if (vndAmount <= 0) return "0.00";

            decimal usd = vndAmount / ExchangeRate;

            // Làm tròn 2 số lẻ
            usd = Math.Round(usd, 2);

            // Kiểm tra tối thiểu 0.01 USD để tránh lỗi số 0
            if (usd < 0.01m) usd = 0.01m;

            // --- SỬA Ở ĐÂY ---
            // Dùng "0.00" thay vì "N2" để loại bỏ dấu phẩy hàng nghìn
            // Ví dụ: 1500 USD sẽ thành "1500.00" (Đúng) thay vì "1,500.00" (Sai)
            return usd.ToString("0", CultureInfo.InvariantCulture);
        }
    }
}