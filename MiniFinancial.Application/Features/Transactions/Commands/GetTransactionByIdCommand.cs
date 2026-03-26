using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Transactions.Commands
{
    public record GetTransactionByIdCommand(Guid Id) : IRequest<Result<TransactionDTO>>;
}
