using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Enums;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")] // Bảo mật trang Admin
    public class DashboardController : BaseAdminController
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IProductVariantService _variantService; // Service lấy biến thể + inventory

        public DashboardController(
            IProductService productService,
            IOrderService orderService,
            IUserService userService,
            IProductVariantService variantService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
            _variantService = variantService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. LẤY DỮ LIỆU TỪ DATABASE
            var products = await _productService.GetAllAsync(includeInactive: true);
            var users = await _userService.GetAllAsync();

            // Lưu ý: OrderService cần đảm bảo lấy được OrderDetails
            var orders = await _orderService.GetAllAsync();

            // Lấy tất cả biến thể (Hàm GetAllAsync này phải Include(Inventory))
            var allVariants = await _variantService.GetAllAsync();

            // 2. TÍNH TOÁN TÀI CHÍNH (CHỈ TÍNH ĐƠN THÀNH CÔNG HOẶC ĐÃ THANH TOÁN)
            var successOrders = orders
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid)
                .ToList();

            decimal totalRevenue = 0;
            decimal totalCost = 0;

            foreach (var order in successOrders)
            {
                totalRevenue += order.TotalAmount; // Cộng doanh thu

                // Tính giá vốn: Lặp qua từng sản phẩm trong đơn -> tìm Variant -> nhân Giá vốn
                if (order.OrderDetails != null)
                {
                    foreach (var item in order.OrderDetails)
                    {
                        // Tìm variant tương ứng trong danh sách đã lấy
                        var variant = allVariants.FirstOrDefault(v => v.Id == item.ProductVariantId);

                        // [SỬA LỖI] Thay variant.ImportPrice bằng variant.Inventory.AverageCost
                        if (variant != null && variant.Inventory != null)
                        {
                            // Lưu ý: Đây là tính toán ước lượng dựa trên giá vốn hiện tại.
                            // Để chính xác tuyệt đối, bảng OrderDetail nên lưu thêm cột CostPrice tại thời điểm bán.
                            totalCost += (variant.Inventory.AverageCost * item.Quantity);
                        }
                    }
                }
            }

            // 3. TÍNH TOÁN BIỂU ĐỒ (7 NGÀY GẦN NHẤT)
            var chartDates = new List<string>();
            var chartValues = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                chartDates.Add(date.ToString("dd/MM")); // Nhãn ngày trục hoành

                // Tổng tiền bán được trong ngày đó
                var dailyRevenue = successOrders
                    .Where(o => o.CreatedAt.Date == date)
                    .Sum(o => o.TotalAmount);

                chartValues.Add(dailyRevenue);
            }

            // 4. TÍNH CẢNH BÁO TỒN KHO (LOW STOCK)
            int lowStockCount = 0;

            // Cấu hình mức cảnh báo chung (Hardcode tạm, sau này lấy từ DB Config)
            const int GLOBAL_MIN_STOCK = 5;

            foreach (var v in allVariants)
            {
                // Kiểm tra null Inventory
                if (v.Inventory != null)
                {
                    // [NÂNG CẤP] Ưu tiên dùng MinStock riêng của sản phẩm, nếu không có thì dùng mức chung
                    int threshold = v.Inventory.MinStock ?? GLOBAL_MIN_STOCK;

                    if (v.Inventory.Quantity <= threshold)
                    {
                        lowStockCount++;
                    }
                }
            }

            // 5. MAP DỮ LIỆU RA VIEW MODEL
            var vm = new AdminDashboardVM
            {
                // Chỉ số cơ bản
                TotalProducts = products.Count(),
                TotalOrders = orders.Count(),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),

                // Tài chính
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                TotalProfit = totalRevenue - totalCost, // Lãi ròng

                // Kho vận
                LowStockCount = lowStockCount,

                // Biểu đồ & Danh sách
                RecentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList(),
                RevenueDates = chartDates,
                RevenueValues = chartValues
            };

            return View(vm);
        }
    }
}