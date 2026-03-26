using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        
        Task AddAsync(Account account, CancellationToken cancellationToken = default);
        
        void Update(Account account);
        
        void Remove(Account account);
    }
}
