namespace MiniFinancial.Application.DTOs
{
    public record CategoryDTO(Guid Id, Guid userId, string Name, decimal? MonthlyLimit);
}
