using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Accounts.Commads
{
    public record UpdateAccountCommand(Guid Id, string Name) : IRequest<Result<AccountDTO>>;
}
