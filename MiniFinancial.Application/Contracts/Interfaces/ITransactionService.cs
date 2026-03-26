using MiniFinancial.Application.Features.Transactions.Commands;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(
            Account accountOrigin,
            Account? accountDestiny,
            RegisterTransactionCommand request,
            DateTimeOffset now,
            CancellationToken cancellationToken);
    }
}
