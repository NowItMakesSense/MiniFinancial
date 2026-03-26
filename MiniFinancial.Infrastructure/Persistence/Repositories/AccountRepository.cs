using Microsoft.EntityFrameworkCore;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Infrastructure.Persistence.Contracts.Services;

namespace MiniFinancial.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts.Include(x => x.Transactions)
                                          .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);
        }

        public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        {
            await _context.Accounts.AddAsync(account, cancellationToken);
        }

        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }

        public void Remove(Account account)
        {
            _context.Accounts.Remove(account);
        }
    }
}
