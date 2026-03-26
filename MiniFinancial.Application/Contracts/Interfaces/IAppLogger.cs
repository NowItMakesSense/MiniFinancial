namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface IAppLogger<T>
    {
        void LogInformation(string message, object? context = null);
        void LogWarning(string message, object? context = null);
        void LogError(Exception exception, string message, object? context = null);
    }
}
