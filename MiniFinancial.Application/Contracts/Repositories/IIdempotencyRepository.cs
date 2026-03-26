using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Repositories
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRequest?> GetByKeyAsync(
            string key,
            CancellationToken cancellationToken = default);

        Task<IdempotencyRequest?> GetByUserIdAndKeyAsync(
            Guid userId,
            string key,
            CancellationToken cancellationToken = default);

        Task AddAsync(IdempotencyRequest request, CancellationToken cancellationToken = default);

        void Update(IdempotencyRequest request);

        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
