using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string email, UserRole role);
    }
}
