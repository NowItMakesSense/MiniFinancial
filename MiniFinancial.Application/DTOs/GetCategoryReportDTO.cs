namespace MiniFinancial.Application.DTOs
{
    public record GetCategoryReportDTO(decimal income, decimal expensive, decimal balance, int countTransactions);
}
