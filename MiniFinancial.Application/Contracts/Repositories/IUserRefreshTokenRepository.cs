using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Repositories
{
    public interface IUserRefreshTokenRepository
    {
        Task<UserRefreshToken?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken);

        Task AddAsync(
            UserRefreshToken refreshToken,
            CancellationToken cancellationToken);

        Task RevokeAllByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken);
    }
}
