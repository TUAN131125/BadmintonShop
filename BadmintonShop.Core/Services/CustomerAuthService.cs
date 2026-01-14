using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;

namespace BadmintonShop.Core.Services
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly IUnitOfWork _uow;

        public CustomerAuthService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<User?> AuthenticateAsync(string account, string password)
        {
            var users = await _uow.UserRepository.GetAllAsync(
                u => (u.Email == account || u.Username == account)
                     && u.IsActive
                     && u.Role.Name == "Customer");

            var user = users.FirstOrDefault();
            if (user == null)
                return null;

            var hash = HashPassword(password);

            return user.PasswordHash == hash
                ? user
                : null;
        }

        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
    }
}
