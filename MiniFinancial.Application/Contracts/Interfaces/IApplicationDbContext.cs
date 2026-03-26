using Microsoft.EntityFrameworkCore;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Account> Accounts { get; }
        DbSet<Transaction> Transactions { get; }
        DbSet<Category> Categories { get; }
        DbSet<User> Users { get; }
        DbSet<IdempotencyRequest> IdempotencyRequests { get; }
        DbSet<UserRefreshToken> UserRefreshTokens { get; }
    }
}
