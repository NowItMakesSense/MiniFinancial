using Microsoft.EntityFrameworkCore;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Contracts.Services
{
    public class AppDbContext : DbContext, IApplicationDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<User> Users => Set<User>();
        public DbSet<IdempotencyRequest> IdempotencyRequests => Set<IdempotencyRequest>();
        public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
