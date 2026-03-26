using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Transactions.Commands
{
    public record UpdateTransactionCommand(Guid Id, string Description) : IRequest<Result<TransactionDTO>>;
}
