using MediatR;
using MiniFinancial.Application.Common;

namespace MiniFinancial.Application.Features.Auth.Commands
{
    public record RevokeRefreshTokenCommand(string RefreshToken) : IRequest<Result>;
}
