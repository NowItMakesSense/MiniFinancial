using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task AddAsync(Category category, CancellationToken cancellationToken = default);

        void Update(Category category);

        void Remove(Category category);
    }
}
