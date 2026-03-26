using Microsoft.AspNetCore.Identity;
using MiniFinancial.Application.Contracts.Interfaces;

namespace MiniFinancial.Application.Contracts.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher;

        public PasswordHasher()
        {
            _hasher = new PasswordHasher<object>();
        }

        public string Hash(Object obj, string password)
        {
            return _hasher.HashPassword(obj, password);
        }

        public bool Verify(Object obj, string password, string hash)
        {
            var result = _hasher.VerifyHashedPassword(obj, hash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
