using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.ValueObjects
{
    public sealed class MonthReference
    {
        public int Year { get; }
        public int Month { get; }

        public MonthReference(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new BusinessRuleException("Mês inválido.");

            Year = year;
            Month = month;
        }
    }

}
