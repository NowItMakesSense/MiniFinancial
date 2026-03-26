using Microsoft.EntityFrameworkCore;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Infrastructure.Persistence.Contracts.Services;

namespace MiniFinancial.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.Include(u => u.RefreshTokens)
                                       .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.Include(u => u.RefreshTokens)
                                       .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }
    }
}
