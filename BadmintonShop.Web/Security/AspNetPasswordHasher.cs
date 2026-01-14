using BadmintonShop.Core.Interfaces.Security;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace BadmintonShop.Web.Security
{
    public class AspNetPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);

            var hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                10000,
                32);

            return Convert.ToBase64String(salt.Concat(hash).ToArray());
        }

        public bool Verify(string password, string stored)
        {
            var bytes = Convert.FromBase64String(stored);
            var salt = bytes[..16];
            var hash = bytes[16..];

            var test = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                10000,
                32);

            return hash.SequenceEqual(test);
        }
    }
}
