using MiniFinancial.Application.Contracts.Interfaces;

namespace MiniFinancial.Application.Contracts.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public DateTimeOffset startOfMonth()
        {
            return new DateTimeOffset(UtcNow.Year, UtcNow.Month, 1, 0, 0, 0, UtcNow.Offset);
        }
    }
}
