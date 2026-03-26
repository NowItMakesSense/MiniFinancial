using Microsoft.EntityFrameworkCore;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Infrastructure.Persistence.Contracts.Services;

namespace MiniFinancial.Infrastructure.Persistence.Repositories
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public UserRefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserRefreshToken?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken)
        {
            return await _context.UserRefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
        }

        public async Task AddAsync(
            UserRefreshToken refreshToken,
            CancellationToken cancellationToken)
        {
            await _context.UserRefreshTokens
                .AddAsync(refreshToken, cancellationToken);
        }

        public async Task RevokeAllByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var tokens = await _context.UserRefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
                token.Revoke();
        }
    }
}
