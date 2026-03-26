namespace MiniFinancial.Application.DTOs
{
    public record GetMonthlyReportDTO(
                  decimal Income,
                  decimal Expense,
                  decimal TransfersWithinUserAccounts,
                  decimal TransfersToOtherAccounts,
                  decimal Balance,
                  decimal BalanceTransfers,
                  int CountIncome,
                  int CountExpense,
                  int CountTransfersWithinUserAccounts,
                  int CountTransfersToOtherAccounts);
}
