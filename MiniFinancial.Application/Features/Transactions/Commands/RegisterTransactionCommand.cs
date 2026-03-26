using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.Features.Transactions.Commands
{
    public record RegisterTransactionCommand(Guid OriginAccountId, Guid? DestinyAccountId, Guid? CategoryUserId, decimal Amount, 
                                             TransactionType Type, TransactionCategory Category, string Description) : IRequest<Result<TransactionDTO>>;

}
