using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleException("Email inválido.");

            Value = value.Trim().ToLowerInvariant();
        }

        public bool Equals(Email? other)
            => other is not null && Value == other.Value;

        public override int GetHashCode()
            => Value.GetHashCode();
    }

}
