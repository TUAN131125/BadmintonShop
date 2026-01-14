using BadmintonShop.Core.Entities;
using System.Collections.Generic;

namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class AdminDashboardVM
    {
        // --- CÁC CHỈ SỐ CƠ BẢN ---
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        // --- CHỈ SỐ TÀI CHÍNH (MỚI) ---
        public decimal TotalRevenue { get; set; } // Doanh thu
        public decimal TotalCost { get; set; }    // Tiền vốn nhập hàng
        public decimal TotalProfit { get; set; }  // Lợi nhuận (Revenue - Cost)

        // --- CẢNH BÁO KHO (MỚI) ---
        public int LowStockCount { get; set; }    // Số lượng SP sắp hết

        // --- DANH SÁCH & BIỂU ĐỒ ---
        public IEnumerable<Order> RecentOrders { get; set; } // 5 đơn mới nhất

        // Dữ liệu vẽ biểu đồ Chart.js
        public List<string> RevenueDates { get; set; }
        public List<decimal> RevenueValues { get; set; }
    }
}