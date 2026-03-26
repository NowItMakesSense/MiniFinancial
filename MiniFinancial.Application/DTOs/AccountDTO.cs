namespace MiniFinancial.Application.DTOs
{
    public record AccountDTO(Guid? Id, Guid UserId, string Name, decimal Balance, bool AllowNegativeBalance);
}
