using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Auth.Commands
{
    public record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginDTO>>;
}
