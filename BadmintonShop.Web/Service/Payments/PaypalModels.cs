using System.Collections.Generic;
using System.Text.Json.Serialization; // Lưu ý namespace này

namespace BadmintonShop.Web.Services.Payments
{
    // --- AUTH ---
    public sealed class AuthResponse
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string app_id { get; set; }
        public int expires_in { get; set; }
        public string nonce { get; set; }
    }

    // --- CREATE ORDER REQUEST ---
    public sealed class CreateOrderRequest
    {
        public string intent { get; set; }
        public List<PurchaseUnit> purchase_units { get; set; } = new();

        // QUAN TRỌNG: Thêm cái này để PayPal biết redirect về đâu
        public ApplicationContext application_context { get; set; }
    }

    public sealed class ApplicationContext
    {
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string brand_name { get; set; }
        public string user_action { get; set; } = "PAY_NOW";
        public string landing_page { get; set; } = "NO_PREFERENCE";
    }

    public sealed class PurchaseUnit
    {
        public Amount amount { get; set; }
        public string reference_id { get; set; }
        public string description { get; set; }
    }

    public class Amount
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }

    // --- RESPONSE CHUNG ---
    public sealed class CreateOrderResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public List<Link> links { get; set; }
    }

    public sealed class CaptureOrderResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        // Các trường khác nếu cần thiết thì thêm vào, hiện tại chỉ cần check status
    }

    public sealed class Link
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }
}