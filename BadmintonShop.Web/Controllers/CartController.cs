using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Helpers;
using BadmintonShop.Web.ViewModels.Cart; // REQUIRED: To use CartItemViewModel
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BadmintonShop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductVariantService _variantService;
        private readonly ICompositeViewEngine _viewEngine;

        public CartController(
            IProductService productService,
            IProductVariantService variantService,
            ICompositeViewEngine viewEngine)
        {
            _productService = productService;
            _variantService = variantService;
            _viewEngine = viewEngine;
        }

        // ==========================================
        // 1. CART PAGE (INDEX) - FIXED TYPE MISMATCH
        // ==========================================
        public IActionResult Index()
        {
            // 1. Get raw data from Session (List<CartItem>)
            var cart = CartSessionHelper.GetCart(HttpContext);

            // 2. Map to ViewModel (List<CartItemViewModel>) to match the View's expectation
            var viewModel = cart.Select(item => new CartItemViewModel
            {
                VariantId = item.VariantId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                Price = item.Price,
                Quantity = item.Quantity,
                // Total is usually a calculated property in ViewModel, so no need to set it here
            }).ToList();

            return View(viewModel);
        }

        // ==========================================
        // 2. ADD TO CART (AJAX)
        // ==========================================
        [HttpPost]
        // Trong method AddToCart [HttpPost]
        public async Task<IActionResult> AddToCart(int productVariantId)
        {
            try
            {
                var variantRaw = await _variantService.GetByIdAsync(productVariantId);
                if (variantRaw == null) return NotFound("Variant not found");

                // Lấy Product cha (bao gồm Variants) để tính giá
                var product = await _productService.GetByIdAsync(variantRaw.ProductId);
                if (product == null) return NotFound("Product not found");

                // *Lưu ý: GetByIdAsync ở trên đã gọi ApplyPromotionLogic, 
                // nên product.Variants đã được điền giá CurrentPrice chính xác.

                var targetVariant = product.Variants.FirstOrDefault(v => v.Id == productVariantId);
                if (targetVariant == null) return NotFound("Variant mismatch");

                // 2. Lấy giỏ hàng
                var cart = CartSessionHelper.GetCart(HttpContext);
                var item = cart.FirstOrDefault(x => x.VariantId == productVariantId);

                if (item == null)
                {
                    item = new CartItem
                    {
                        VariantId = targetVariant.Id,
                        ProductId = product.Id,
                        ProductName = $"{product.Name} ({targetVariant.AttributeValue})",
                        // [QUAN TRỌNG] Lấy giá CurrentPrice (đã trừ khuyến mãi)
                        Price = targetVariant.CurrentPrice,
                        Quantity = 1,
                        ImageUrl = product.ImageUrl
                    };
                    cart.Add(item);
                }
                else
                {
                    item.Quantity++;
                    // Cập nhật lại giá mới nhất luôn cho chắc ăn
                    item.Price = targetVariant.CurrentPrice;
                }

                // ... Lưu session và trả về JSON như cũ ...
                CartSessionHelper.SaveCart(HttpContext, cart);
                var miniCartHtml = await RenderViewAsync("_MiniCart", cart);
                return Json(new { success = true, count = cart.Sum(x => x.Quantity), html = miniCartHtml });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ==========================================
        // 3. REMOVE ITEM
        // ==========================================
        public IActionResult Remove(int id)
        {
            var cart = CartSessionHelper.GetCart(HttpContext);

            // Find item by VariantId
            var item = cart.FirstOrDefault(x => x.VariantId == id);

            if (item != null)
            {
                cart.Remove(item);
                CartSessionHelper.SaveCart(HttpContext, cart);
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 4. UPDATE QUANTITY
        // ==========================================
        public IActionResult Update(int id, int quantity)
        {
            var cart = CartSessionHelper.GetCart(HttpContext);

            var item = cart.FirstOrDefault(x => x.VariantId == id);

            if (item != null)
            {
                item.Quantity = quantity;
                CartSessionHelper.SaveCart(HttpContext, cart);
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // HELPER: RENDER VIEW TO STRING
        // ==========================================
        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            // Render lại MiniCart View
            var miniCartHtml = await RenderViewAsync("_MiniCart", cart);

            return Json(new
            {
                count = cart.Sum(x => x.Quantity),
                html = miniCartHtml
            });
        }
    }
}