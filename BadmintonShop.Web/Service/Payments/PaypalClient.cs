using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; // Cần cái này cho PostAsJsonAsync
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Services.Payments
{
    public sealed class PaypalClient
    {
        public string Mode { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }

        public string BaseUrl => Mode == "Live"
            ? "https://api-m.paypal.com"
            : "https://api-m.sandbox.paypal.com";

        public PaypalClient(string clientId, string clientSecret, string mode)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Mode = mode;
        }

        private async Task<AuthResponse> Authenticate()
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
            var content = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials")
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{BaseUrl}/v1/oauth2/token"),
                Method = HttpMethod.Post,
                Headers = { { "Authorization", $"Basic {auth}" } },
                Content = new FormUrlEncodedContent(content)
            };

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(request);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception("Auth Failed: " + await httpResponse.Content.ReadAsStringAsync());
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponse>(jsonResponse);
        }

        public async Task<CreateOrderResponse> CreateOrder(string value, string currency, string reference, string returnUrl, string cancelUrl)
        {
            var auth = await Authenticate();

            var request = new CreateOrderRequest
            {
                intent = "CAPTURE",
                purchase_units = new List<PurchaseUnit>
                {
                    new()
                    {
                        reference_id = reference,
                        description = $"Order {reference}",
                        amount = new Amount
                        {
                            currency_code = currency,
                            value = value
                        }
                    }
                },
                // Thêm phần này để Redirect hoạt động
                application_context = new ApplicationContext
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl,
                    brand_name = "Badminton Shop"
                }
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {auth.access_token}");

            var httpResponse = await httpClient.PostAsJsonAsync($"{BaseUrl}/v2/checkout/orders", request);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync();
                throw new Exception($"Create Order Failed: {err}");
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CreateOrderResponse>(jsonResponse);
        }

        public async Task<CaptureOrderResponse> CaptureOrder(string orderId)
        {
            var auth = await Authenticate();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {auth.access_token}");

            var httpContent = new StringContent("{}", Encoding.UTF8, "application/json");
            var httpResponse = await httpClient.PostAsync($"{BaseUrl}/v2/checkout/orders/{orderId}/capture", httpContent);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync();
                throw new Exception($"Capture Failed: {err}");
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CaptureOrderResponse>(jsonResponse);
        }
    }
}