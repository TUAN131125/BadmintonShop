# 🏸 Badminton Shop - Hệ thống Kinh doanh Vợt Cầu lông

![Badges](https://img.shields.io/badge/Framework-.NET%20Core%208.0-purple)
![Badges](https://img.shields.io/badge/Database-SQL%20Server-blue)
![Badges](https://img.shields.io/badge/Payment-PayPal%20Integration-00457C)
![Badges](https://img.shields.io/badge/Status-Completed-success)

> **Đồ án cơ sở**
> 
> Một hệ thống thương mại điện tử chuyên biệt cho lĩnh vực cầu lông, giải quyết các bài toán về sản phẩm đa biến thể (Màu sắc, kích thước, trọng lượng) và quản lý kho hàng chặt chẽ.

## 🚀 Giới thiệu

Badminton Shop là một ứng dụng web được xây dựng trên nền tảng **ASP.NET Core 8.0 MVC**. Hệ thống cung cấp trải nghiệm mua sắm mượt mà cho khách hàng và công cụ quản trị mạnh mẽ cho chủ cửa hàng, tập trung vào việc xử lý tính toàn vẹn của dữ liệu tồn kho và thanh toán trực tuyến.

## 🛠️ Công nghệ sử dụng

* **Back-end:** ASP.NET Core 8.0 (C#)
* **Front-end:** HTML5, CSS3, Bootstrap 5, jQuery (AJAX).
* **Database:** Microsoft SQL Server (Entity Framework Core - Code First).
* **Payment:** PayPal RESTful API v2 (Checkout SDK).
* **Tools:** Visual Studio 2022, SQL Server Management Studio (SSMS).

## ✨ Tính năng nổi bật

### 🛒 Phân hệ Khách hàng (Store Front-end)
- [x] **Tìm kiếm & Lọc:** Tìm kiếm theo từ khóa, lọc theo giá, thương hiệu, danh mục đệ quy.
- [x] **Sản phẩm Đa biến thể:** Xem chi tiết sản phẩm, lựa chọn thuộc tính (Size, Màu, 3U/4U) -> Giá và Tồn kho cập nhật tức thì (Real-time).
- [x] **Giỏ hàng & Đặt hàng:** Quản lý giỏ hàng (Session), Quy trình Checkout tối ưu.
- [x] **Thanh toán PayPal:** Tích hợp cổng thanh toán quốc tế PayPal (Sandbox).
- [x] **Quản lý Tài khoản:** Theo dõi lịch sử đơn hàng, xem trạng thái xử lý đơn.

### 🔐 Phân hệ Quản trị (Admin Panel)
- [x] **Quản lý Danh mục Đệ quy:** Hỗ trợ danh mục đa cấp (Cha - Con) không giới hạn.
- [x] **Quản lý Sản phẩm chuyên sâu:** Thêm/Sửa/Xóa sản phẩm và quản lý danh sách biến thể (SKU) chi tiết.
- [x] **Quản lý Kho hàng & Nhật ký (Audit Log):** Ghi vết mọi thao tác nhập/xuất/điều chỉnh kho. Tự động tính giá vốn bình quân.
- [x] **Xử lý Đơn hàng:** Quy trình duyệt đơn, cập nhật trạng thái, in hóa đơn.
- [x] **Marketing:** Quản lý Banner Slider, Tin tức, Chương trình khuyến mãi.

## ⚙️ Hướng dẫn Cài đặt & Chạy dự án

Để chạy được dự án trên máy cục bộ, vui lòng làm theo các bước sau:

### 1. Yêu cầu môi trường
* .NET 8.0 SDK trở lên.
* SQL Server 2019 trở lên.
* Visual Studio 2022 (Community hoặc Enterprise).

### 2. Cấu hình Cơ sở dữ liệu (Database)
Trong thư mục gốc của dự án (hoặc thư mục `Database`), tôi đã cung cấp file script SQL: **`BadmintonShopDB.sql`**.

Vui lòng thực hiện các bước sau để khôi phục dữ liệu:
1.  Mở **SQL Server Management Studio (SSMS)** và kết nối vào Server của bạn.
2.  Tạo một Database mới (New Database) và đặt tên chính xác là: **`BadmintonShopDB`**.
3.  Trên thanh menu, chọn **File** -> **Open** -> **File...** và chọn file `BadmintonShop_DB.sql` đã tải về.
4.  Tại giao diện câu lệnh, hãy đảm bảo ô chọn Database (Dropdown list góc trái trên cùng) đang trỏ vào `BadmintonShopDB`.
5.  Nhấn nút **Execute** (hoặc phím F5) để chạy script.

> **Thông tin đăng nhập Admin mặc định:**
> * Tài khoản: `admin` (hoặc email admin)
> * Mật khẩu: `admin` (Vui lòng kiểm tra trong bảng Users nếu mật khẩu khác).

### 3. Cấu hình kết nối (Appsettings)
Mở file `appsettings.json` trong project và cập nhật lại thông tin kết nối và PayPal:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=BadmintonShopDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "PayPal": {
    "Mode": "Sandbox",
    "ClientId": "YOUR_PAYPAL_CLIENT_ID",
    "ClientSecret": "YOUR_PAYPAL_CLIENT_SECRET"
  }
}
