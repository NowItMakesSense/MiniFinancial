using MiniFinancial.Domain.Commom;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid OriginAccountId { get; private set; }

        public Guid? DestinyAccountId { get; private set; }

        public Guid? CategoryUserId { get; private set; }

        public string? CategoryUserName { get; private set; }

        public decimal Amount { get; private set; }

        public TransactionType Type { get; private set; }

        public TransactionCategory Category { get; private set; }

        public string Description { get; private set; }

        public DateTimeOffset OccurredAt { get; private set; }

        public bool IsRecurring { get; private set; }

        public bool? IsReversal { get; private set; }

        public Guid? ReversedTransactionId { get; private set; }

        public Transaction() { }

        public Transaction(Guid originAccountId, Guid? destinyAccountId, Guid? categoryUserId, string? categoryUserName, decimal amount, TransactionType type,
                           TransactionCategory category, string description, DateTimeOffset occurredAt, bool isRecurring = false) : base(occurredAt)
        {
            if (originAccountId == Guid.Empty) throw new BusinessRuleException("Conta inválida.");
            if (destinyAccountId == Guid.Empty && categoryUserId == Guid.Empty) throw new BusinessRuleException("Categoria inválida.");
            if (amount <= 0) throw new BusinessRuleException("Valor da transação deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(description)) throw new BusinessRuleException("Descrição é obrigatória.");
            if (occurredAt > DateTimeOffset.UtcNow.AddMinutes(5)) throw new BusinessRuleException("Data da transação inválida.");

            OriginAccountId = originAccountId;
            DestinyAccountId = destinyAccountId;
            CategoryUserId = categoryUserId;
            CategoryUserName = categoryUserName;
            Amount = amount;
            Type = type;
            Category = category;
            Description = description.Trim();
            OccurredAt = occurredAt;
            IsRecurring = isRecurring;
        }
        public void Delete(DateTimeOffset now)
        {
            if (IsDeleted) return;

            MarkAsDeleted(now);
        }

        public Transaction Reverse(DateTimeOffset now)
        {
            if (IsDeleted) throw new BusinessRuleException("Não é possível reverter uma transação excluída.");

            var newType = TransactionType.Transfer;
            if (this.Type != newType) newType = this.Type == TransactionType.Expense ? TransactionType.Income : TransactionType.Expense;

            var newTransaction = new Transaction(
                OriginAccountId,
                DestinyAccountId,
                CategoryUserId,
                CategoryUserName,
                Amount,
                Type = newType,
                Category,
                Description,
                now
            );

            return newTransaction;
        }

        public void SetReversal(bool value, DateTimeOffset now)
        {
            IsReversal = value;
            MarkAsUpdated(now);
        }

        public void SetReversedTransactionId(Guid id, DateTimeOffset now)
        {
            ReversedTransactionId = id;
            MarkAsUpdated(now);
        }

        public void SetDescription(string description, DateTimeOffset now)
        {
            Description = description;
            MarkAsUpdated(now);
        }

        public decimal GetSignedAmount() => (Type == TransactionType.Expense || Type == TransactionType.Transfer) ? -Amount : Amount;
    }
}
