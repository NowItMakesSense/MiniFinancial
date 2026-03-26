namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }

        DateTimeOffset startOfMonth();
    }
}
