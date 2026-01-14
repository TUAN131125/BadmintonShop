using System.Security.Cryptography;
using System.Text;
using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces;
using BadmintonShop.Core.Interfaces.Services;

namespace BadmintonShop.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;

        public AuthService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _uow.UserRepository.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
                return null;

            return user.PasswordHash == HashPassword(password)
                ? user
                : null;
        }
    }
}
