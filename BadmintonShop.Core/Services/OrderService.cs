using BadmintonShop.Core.DTOs;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Enums;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore; // [NEW] Cần thiết cho Include/ToListAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;
        private readonly IUserService _userService;
        private readonly IProductService _productService; // [NEW] Inject ProductService

        public OrderService(
            IUnitOfWork unitOfWork,
            IInventoryService inventoryService,
            IUserService userService,
            IProductService productService) // [NEW] Add param
        {
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
            _userService = userService;
            _productService = productService;
        }

        /// <summary>
        /// Xử lý logic đặt hàng (Checkout)
        /// </summary>
        public async Task<Order> CheckoutAsync(CheckoutDTO checkoutDto)
        {
            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
                throw new Exception("Cart cannot be empty.");

            User user;
            // 1. Xác định User: Nếu có ID thì lấy từ DB, nếu không thì tìm/tạo theo Email (cho khách vãng lai)
            if (checkoutDto.UserId != null)
            {
                user = await _unitOfWork.UserRepository.GetByIdAsync(checkoutDto.UserId.Value)
                       ?? throw new Exception("User not found.");
            }
            else
            {
                user = await _userService.FindOrCreateAsync(
                    checkoutDto.Email,
                    checkoutDto.FullName,
                    checkoutDto.Phone
                );
            }

            // 2. Khởi tạo đối tượng Order
            var order = new Order
            {
                UserId = user.Id,
                CustomerName = checkoutDto.FullName,
                Phone = checkoutDto.Phone,
                Email = checkoutDto.Email,
                ShippingAddress = checkoutDto.ShippingAddress,
                ShippingFee = checkoutDto.ShippingFee,
                PaymentMethod = checkoutDto.PaymentMethod,
                Country = "Vietnam",
                City = checkoutDto.City,
                Ward = checkoutDto.Ward,
                TotalAmount = 0,
                IsPaid = false,
                Status = checkoutDto.PaymentMethod == "PAYPAL" ? OrderStatus.AwaitingPayment : OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>(),
                OrderHistories = new List<OrderHistory>()
            };

            decimal total = 0;

            // ==================================================================================
            // 3. [UPDATED LOGIC] Lấy sản phẩm hàng loạt và Tính toán giá khuyến mãi
            // ==================================================================================

            // 3.1. Lấy danh sách ID sản phẩm cần mua từ giỏ hàng
            var variantIds = checkoutDto.Items.Select(i => i.ProductVariantId).Distinct().ToList();

            // 3.2. Load các Variant từ DB (Kèm Product cha)
            // Sử dụng GetQuery() để có thể dùng Include
            var dbVariants = await _unitOfWork.ProductVariantRepository.GetQuery()
                .Include(v => v.Product)
                //.ThenInclude(p => p.Promotions) // Nếu Promotion nằm trong Product thì nên Include thêm
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync();

            if (!dbVariants.Any()) throw new Exception("No products found for these items.");

            // 3.3. Gom nhóm theo Product để tính giá khuyến mãi (Vì Promotion thường áp dụng theo Product)
            var products = dbVariants.Select(v => v.Product).Distinct().ToList();

            // Gọi ProductService để tính giá cho list Product này (Update field CurrentPrice trong RAM)
            await _productService.ApplyPromotionLogic(products);

            // 3.4. Duyệt qua từng item trong giỏ để tạo OrderDetail
            foreach (var item in checkoutDto.Items)
            {
                var variantEntity = dbVariants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                if (variantEntity == null) throw new Exception($"Product Variant ID {item.ProductVariantId} not found");

                // Kiểm tra tồn kho
                var inventory = await _unitOfWork.InventoryRepository.GetByVariantIdAsync(item.ProductVariantId);
                if (inventory == null || inventory.Quantity < item.Quantity)
                    throw new Exception($"Product '{variantEntity.Product?.Name ?? "Unknown"}' is out of stock.");

                // [QUAN TRỌNG] Lấy giá CurrentPrice đã được tính toán ở bước 3.3
                // Do ApplyPromotionLogic thao tác trên object 'products', ta cần lấy giá từ object đó
                var parentProduct = products.First(p => p.Id == variantEntity.ProductId);

                // Tìm variant tương ứng trong object Product đã được tính toán
                // Lưu ý: parentProduct.Variants cần được populate, nếu EF Core Tracking hoạt động tốt 
                // thì nó sẽ chứa các variant đã load ở bước 3.2
                var calculatedVariant = parentProduct.Variants?.FirstOrDefault(v => v.Id == variantEntity.Id);

                // Nếu không lấy được giá đã tính (do object reference khác nhau hoặc null), fallback về giá gốc
                decimal finalPrice = calculatedVariant != null ? calculatedVariant.CurrentPrice : variantEntity.Price;

                // Nếu giá vẫn bằng 0 (chưa set), lấy BasePrice
                if (finalPrice == 0) finalPrice = variantEntity.Product?.BasePrice ?? 0;

                total += finalPrice * item.Quantity;

                // Thêm vào chi tiết đơn hàng
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = finalPrice, // Giá đã giảm (nếu có)
                    ProductName = variantEntity.Product?.Name ?? "N/A",
                    SKU = variantEntity.SKU
                });
            }

            order.TotalAmount = total;

            // 4. Lưu đơn hàng vào DB
            await _unitOfWork.OrderRepository.AddAsync(order);
            await _unitOfWork.SaveAsync();

            // 5. Trừ tồn kho sau khi tạo đơn thành công
            foreach (var item in checkoutDto.Items)
            {
                await _inventoryService.DecreaseStockAsync(
                    item.ProductVariantId,
                    item.Quantity,
                    "Order placed by customer",
                    user.Id,
                    order.Id
                );
            }

            return order;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của User (Sắp xếp mới nhất trước)
        /// </summary>
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(o => o.UserId == userId);
            return orders.OrderByDescending(o => o.CreatedAt);
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng theo ID
        /// </summary>
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            // Giả định Repository đã có hàm GetOrderDetailAsync bao gồm (Include) các bảng con
            return await _unitOfWork.OrderRepository.GetOrderDetailAsync(orderId);
        }

        /// <summary>
        /// Lấy tất cả đơn hàng (Admin)
        /// </summary>
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                includeProperties: "OrderDetails"
            );
            return orders.OrderByDescending(o => o.CreatedAt);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) throw new Exception("Order not found.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Hủy đơn hàng và hoàn lại kho
        /// </summary>
        public async Task CancelAsync(int orderId, int userId, string reason)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderDetailAsync(orderId);
            if (order == null) throw new Exception("Order not found.");

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                throw new Exception("Cannot cancel this order (Completed or already Cancelled).");

            // 1. Hoàn lại tồn kho cho từng sản phẩm
            foreach (var detail in order.OrderDetails)
            {
                // Lấy thông tin kho hiện tại để lấy Giá vốn bình quân (AverageCost)
                var currentInventory = await _inventoryService.GetStockByVariantIdAsync(detail.ProductVariantId);
                decimal returnCost = currentInventory != null ? currentInventory.AverageCost : 0;

                await _inventoryService.IncreaseStockAsync(
                    detail.ProductVariantId,
                    detail.Quantity,
                    $"Order #{orderId} Cancelled: {reason}",
                    returnCost,
                    InventoryActionType.Import,
                    userId
                );
            }

            // 2. Cập nhật trạng thái đơn hàng
            order.Status = OrderStatus.Cancelled;
            order.Note = reason;
            order.UpdatedAt = DateTime.UtcNow;

            // 3. Ghi lịch sử đơn hàng
            if (order.OrderHistories == null) order.OrderHistories = new List<OrderHistory>();
            order.OrderHistories.Add(new OrderHistory
            {
                Status = OrderStatus.Cancelled,
                Note = $"Cancelled by Admin/User: {reason}",
                ModifiedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveAsync();
        }
    }
}