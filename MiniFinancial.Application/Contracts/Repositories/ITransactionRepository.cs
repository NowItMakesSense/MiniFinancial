using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Transaction>> GetByPeriodAsync(
            Guid accountId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            CancellationToken cancellationToken = default);

        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

        void Update(Transaction transaction);

        void Remove(Transaction transaction);
    }
}
