using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, Guid sessionId);
        string GenerateRefreshToken();
    }
}
