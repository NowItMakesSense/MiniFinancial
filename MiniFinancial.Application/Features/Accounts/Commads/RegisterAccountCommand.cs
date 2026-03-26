using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Accounts.Commads
{
    public record RegisterAccountCommand(Guid UserId, string Name) : IRequest<Result<AccountDTO>>;
}
