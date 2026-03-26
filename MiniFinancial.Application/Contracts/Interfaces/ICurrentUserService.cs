namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        bool IsAdmin { get; }
    }
}
