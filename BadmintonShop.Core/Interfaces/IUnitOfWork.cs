using BadmintonShop.Core.Interfaces.Repositories;

namespace BadmintonShop.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Product & Catalog
        ICategoryRepository CategoryRepository { get; }
        IProductRepository ProductRepository { get; }
        IProductVariantRepository ProductVariantRepository { get; }

        // Inventory & Orders
        IInventoryRepository InventoryRepository { get; }
        IOrderRepository OrderRepository { get; }

        // Users & Roles
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }

        // Marketing
        IBannerRepository BannerRepository { get; }
        INewsRepository NewsRepository { get; }
        IPromotionRepository PromotionRepository { get; }

        // Commit database changes
        Task<int> SaveAsync();
    }
}
