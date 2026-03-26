using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Auth.Commands
{
    public record LogoutCommand(string RefreshToken) : IRequest<Result<LoginDTO>>;
}
