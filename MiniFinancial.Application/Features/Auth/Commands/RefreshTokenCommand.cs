using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginDTO>>;
}
