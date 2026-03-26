using Microsoft.EntityFrameworkCore;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Infrastructure.Persistence.Contracts.Services;

namespace MiniFinancial.Infrastructure.Persistence.Repositories
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly AppDbContext _context;

        public IdempotencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IdempotencyRequest?> GetByKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return await _context.IdempotencyRequests
                .FirstOrDefaultAsync(x => x.Key == key && !x.IsDeleted, cancellationToken);
        }

        public async Task<IdempotencyRequest?> GetByUserIdAndKeyAsync(
            Guid userId,
            string key,
            CancellationToken cancellationToken = default)
        {
            return await _context.IdempotencyRequests
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Key == key && !x.IsDeleted, cancellationToken);
        }

        public async Task AddAsync(
            IdempotencyRequest request,
            CancellationToken cancellationToken = default)
        {
            await _context.IdempotencyRequests.AddAsync(request, cancellationToken);
        }

        public void Update(IdempotencyRequest request)
        {
            _context.IdempotencyRequests.Update(request);
        }

        public async Task<bool> ExistsAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return await _context.IdempotencyRequests
                .AnyAsync(x => x.Key == key && !x.IsDeleted, cancellationToken);
        }
    }
}
