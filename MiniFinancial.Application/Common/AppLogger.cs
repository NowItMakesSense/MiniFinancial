using Microsoft.Extensions.Logging;
using MiniFinancial.Application.Contracts.Interfaces;

namespace MiniFinancial.Application.Common
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public AppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, object? context = null)
        {
            if (context != null)
                _logger.LogInformation("{Message} {@Context}", message, context);
            else
                _logger.LogInformation(message);
        }

        public void LogWarning(string message, object? context = null)
        {
            if (context != null)
                _logger.LogWarning("{Message} {@Context}", message, context);
            else
                _logger.LogWarning(message);
        }

        public void LogError(Exception exception, string message, object? context = null)
        {
            if (context != null)
                _logger.LogError(exception, "{Message} {@Context}", message, context);
            else
                _logger.LogError(exception, message);
        }
    }
}
