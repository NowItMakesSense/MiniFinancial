using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.ValueObjects
{
    public sealed class IdempotencyKey
    {
        public string Value { get; }

        public IdempotencyKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleException("Chave inválida.");

            Value = value.Trim();
        }
    }
}
