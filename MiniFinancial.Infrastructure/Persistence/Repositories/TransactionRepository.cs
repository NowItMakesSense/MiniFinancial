using Microsoft.EntityFrameworkCore;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Infrastructure.Persistence.Contracts.Services;

namespace MiniFinancial.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(x => x.OriginAccountId == accountId && !x.IsDeleted)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByPeriodAsync(
            Guid accountId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(x =>
                    x.OriginAccountId == accountId &&
                    x.OccurredAt >= startDate &&
                    x.OccurredAt <= endDate &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.Transactions.AddAsync(transaction, cancellationToken);
        }

        public void Update(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
        }

        public void Remove(Transaction transaction)
        {
            _context.Transactions.Remove(transaction);
        }
    }
}
