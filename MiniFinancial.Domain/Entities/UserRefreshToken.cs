using MiniFinancial.Domain.Commom;

namespace MiniFinancial.Domain.Entities
{
    public sealed class UserRefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Token { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }

        public bool IsRevoked { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }

        public string? ReplacedByToken { get; private set; }

        private UserRefreshToken() { }

        public UserRefreshToken(Guid userId, string token, DateTimeOffset expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public bool IsActive() => !IsRevoked && DateTimeOffset.UtcNow <= ExpiresAt;

        public void Revoke(string? replacedByToken = null)
        {
            IsRevoked = true;
            RevokedAt = DateTimeOffset.UtcNow;
            ReplacedByToken = replacedByToken;
        }
    }
}
