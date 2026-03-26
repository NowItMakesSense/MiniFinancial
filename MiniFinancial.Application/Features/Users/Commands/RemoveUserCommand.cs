using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Users.Commands
{
    public record RemoveUserCommand(Guid Id) : IRequest<Result<UserDTO>>;
}
