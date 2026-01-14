using BadmintonShop.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.DbContext
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Banner> Banners { get; set; }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed dữ liệu Role
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Customer" }
            );

            // Cấu hình Category (Đệ quy cha-con)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // CẤU HÌNH NHIỀU - NHIỀU (PRODUCT - CATEGORY)
            // =========================================================
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Products)
                .UsingEntity(j => j.ToTable("ProductCategories"));

            // Cấu hình Inventory 1-1 với Variant
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Inventory)
                .WithOne(i => i.ProductVariant)
                .HasForeignKey<Inventory>(i => i.ProductVariantId);

            // =========================================================
            // QUAN TRỌNG: CẤU HÌNH TIỀN TỆ (DECIMAL)
            // =========================================================

            // 1. Bảng Product
            modelBuilder.Entity<Product>(e => {
                e.Property(p => p.BasePrice).HasColumnType("decimal(18, 2)");
                e.Property(p => p.SalePrice).HasColumnType("decimal(18, 2)");
            });

            // 2. Bảng ProductVariant
            modelBuilder.Entity<ProductVariant>(e => {
                e.Property(p => p.Price).HasColumnType("decimal(18, 2)");
                // [ĐÃ SỬA] Xóa dòng cấu hình ImportPrice vì trường này đã bị xóa khỏi Entity
            });

            // 3. Bảng Inventory [MỚI]
            modelBuilder.Entity<Inventory>(e => {
                // Cấu hình AverageCost để lưu giá vốn chính xác
                e.Property(i => i.AverageCost).HasColumnType("decimal(18, 2)");
            });

            // 4. Bảng InventoryLog [MỚI]
            modelBuilder.Entity<InventoryLog>(e => {
                // Cấu hình CostPerUnit để lưu lịch sử giá nhập/xuất
                e.Property(l => l.CostPerUnit).HasColumnType("decimal(18, 2)");
            });

            // 5. Bảng Order
            modelBuilder.Entity<Order>(e => {
                e.Property(p => p.ShippingFee).HasColumnType("decimal(18, 2)");
                e.Property(p => p.TotalAmount).HasColumnType("decimal(18, 2)");

                // Quan hệ 1-Nhiều với OrderHistory
                e.HasMany(o => o.OrderHistories)
                 .WithOne(h => h.Order)
                 .HasForeignKey(h => h.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Bảng OrderDetail
            modelBuilder.Entity<OrderDetail>(e => {
                e.Property(p => p.UnitPrice).HasColumnType("decimal(18, 2)");
            });

            // 7. Cấu hình Review (Xóa sản phẩm thì xóa luôn review)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.PromotionProducts)
                .WithOne(pp => pp.Promotion)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Promotion thì xóa luôn liên kết

            modelBuilder.Entity<PromotionProduct>()
                .HasKey(pp => new { pp.PromotionId, pp.ProductId }); // Khóa chính phức hợp

            modelBuilder.Entity<PromotionProduct>()
                .HasOne(pp => pp.Product)
                .WithMany() // Product không cần giữ list PromotionProducts nếu không cần thiết
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình tiền tệ cho Promotion
            modelBuilder.Entity<Promotion>(e => {
                e.Property(p => p.DiscountValue).HasColumnType("decimal(18, 2)");
            });
        }
    }
}