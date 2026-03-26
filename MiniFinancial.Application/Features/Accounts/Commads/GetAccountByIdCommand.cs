using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Accounts.Commads
{
    public record GetAccountByIdCommand(Guid Id) : IRequest<Result<AccountDTO>>;
}
