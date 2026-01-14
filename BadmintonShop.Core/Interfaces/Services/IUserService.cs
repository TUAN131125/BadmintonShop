using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task CreateAsync(User user, string password);
        Task UpdateAsync(User user);
        Task LockAsync(int id);
        Task UnlockAsync(int id);

        Task<IEnumerable<User>> GetAdminsAsync();
        Task<IEnumerable<User>> GetCustomersAsync();

        // Dùng cho Guest Checkout (mua hàng không cần đăng nhập)
        Task<User> FindOrCreateAsync(string email, string fullName, string phone);

        // Đăng ký tài khoản mới (hoặc nâng cấp từ Guest)
        Task<User> RegisterCustomerAsync(string fullName, string email, string phone, string address, string province,string password);

        // Xác thực đăng nhập
        Task<User?> AuthenticateAsync(string email, string password);
    }
}