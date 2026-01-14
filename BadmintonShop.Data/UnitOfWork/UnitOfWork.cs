using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Repositories;
using BadmintonShop.Data.DbContext;
using BadmintonShop.Data.Repositories;
using BadmintonShop.Data.Repositories.Implementations;

namespace BadmintonShop.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IProductRepository ProductRepository { get; }
        public IProductVariantRepository ProductVariantRepository { get; }
        public IInventoryRepository InventoryRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IUserRepository UserRepository { get; }
        public IRoleRepository RoleRepository { get; }

        // Marketing
        public IBannerRepository BannerRepository { get; }
        public INewsRepository NewsRepository { get; }

        public IPromotionRepository PromotionRepository { get; }
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            ProductRepository = new ProductRepository(_context);
            ProductVariantRepository = new ProductVariantRepository(_context);
            InventoryRepository = new InventoryRepository(_context);
            OrderRepository = new OrderRepository(_context);
            CategoryRepository = new CategoryRepository(_context);
            UserRepository = new UserRepository(_context);
            RoleRepository = new RoleRepository(_context);
            BannerRepository = new BannerRepository(_context);
            NewsRepository = new NewsRepository(_context);
            PromotionRepository = new PromotionRepository(_context);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
