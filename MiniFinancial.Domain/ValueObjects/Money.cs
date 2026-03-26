using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.ValueObjects
{
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }

        public Money(decimal amount)
        {
            if (amount < 0)
                throw new BusinessRuleException("Valor monetário inválido.");

            Amount = amount;
        }

        public bool Equals(Money? other)
            => other is not null && Amount == other.Amount;

        public override int GetHashCode()
            => Amount.GetHashCode();
    }
}
