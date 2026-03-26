using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.Features.Users.Commands
{
    public record RegisterUserCommand(string Name, string Email, string Password, UserRole Role) : IRequest<Result<UserDTO>>;
}
