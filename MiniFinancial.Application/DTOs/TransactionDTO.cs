using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.DTOs
{
    public record TransactionDTO(Guid Id, Guid OriginAccountId, Guid? DestinyAccountId, Guid? CategoryUserId, string? CategoryUserName, 
                                 decimal Amount, TransactionType Type, TransactionCategory Category, string Description, DateTimeOffset OccurredAt,
                                 bool IsRecurring, bool? IsReversal, Guid? ReversedTransactionId);
}
