using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BadmintonShop.Web.Helpers // Hoặc namespace hiện tại của bạn
{
    public static class CartSessionHelper
    {
        private const string KEY = "CART";

        // ========== CORE METHODS (Lấy/Lưu Session) ==========
        public static List<CartItem> GetCart(HttpContext ctx)
        {
            var json = ctx.Session.GetString(KEY);
            return json == null
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(json);
        }

        public static void SaveCart(HttpContext ctx, List<CartItem> cart)
        {
            ctx.Session.SetString(KEY, JsonConvert.SerializeObject(cart));
        }

        public static void ClearCart(HttpContext ctx)
        {
            ctx.Session.Remove(KEY);
        }

        // ========== CART OPERATIONS (Thao tác giỏ hàng) ==========

        // 1. Thêm vào giỏ (Logic theo VariantId)
        public static void AddItem(HttpContext ctx, CartItem item)
        {
            var cart = GetCart(ctx);

            // Tìm xem biến thể này (VD: 4U) đã có trong giỏ chưa
            var existing = cart.FirstOrDefault(x => x.VariantId == item.VariantId);

            if (existing == null)
            {
                cart.Add(item);
            }
            else
            {
                existing.Quantity += item.Quantity;
            }

            SaveCart(ctx, cart);
        }

        // 2. Cập nhật số lượng (Logic theo VariantId)
        public static void UpdateQuantity(HttpContext ctx, int variantId, int quantity)
        {
            var cart = GetCart(ctx);

            var item = cart.FirstOrDefault(x => x.VariantId == variantId);
            if (item == null) return;

            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;

            SaveCart(ctx, cart);
        }

        // 3. Xóa sản phẩm (Logic theo VariantId)
        public static void RemoveItem(HttpContext ctx, int variantId)
        {
            var cart = GetCart(ctx);

            // Xóa đúng biến thể đó
            cart.RemoveAll(x => x.VariantId == variantId);

            SaveCart(ctx, cart);
        }
    }

    // ========== CART ITEM CLASS (Đã cập nhật) ==========
    public class CartItem
    {
        // Quan trọng: Phải có VariantId để phân biệt các size/màu khác nhau
        public int VariantId { get; set; }

        public int ProductId { get; set; }

        // Đổi tên cho khớp với Controller (ProductName thay vì Name)
        public string ProductName { get; set; }

        // Đổi tên cho khớp với Controller (ImageUrl thay vì Image)
        public string ImageUrl { get; set; }

        public decimal Price { get; set; }

        // Đổi tên cho khớp với Controller (Quantity thay vì Qty)
        public int Quantity { get; set; }

        // Tính tổng tiền tiện lợi
        public decimal Total => Price * Quantity;
    }
}