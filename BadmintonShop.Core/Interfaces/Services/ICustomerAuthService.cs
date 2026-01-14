using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface ICustomerAuthService
    {
        Task<User?> AuthenticateAsync(string account, string password);
    }
}
