using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);

        string HashPassword(string password);
    }
}
