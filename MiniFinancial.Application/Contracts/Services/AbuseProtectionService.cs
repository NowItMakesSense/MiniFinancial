using Microsoft.Extensions.Caching.Memory;

namespace MiniFinancial.Application.Contracts.Services
{
    using Microsoft.Extensions.Caching.Memory;

    public class AbuseProtectionService
    {
        private readonly IMemoryCache _cache;

        public AbuseProtectionService(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static string FailKey(string key) => $"abuse:fail:{key}";
        private static string BanKey(string key) => $"abuse:ban:{key}";

        public bool IsBanned(string key) => _cache.TryGetValue(BanKey(key), out _);

        public (bool blocked, TimeSpan? retryAfter, int attempts) RegisterFailure(string key)
        {
            var attempts = _cache.GetOrCreate(FailKey(key), entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return 0;
            });

            attempts++;
            _cache.Set(FailKey(key), attempts, TimeSpan.FromMinutes(15));

            if (attempts >= 20) return Ban(key, TimeSpan.FromHours(1), attempts);
            if (attempts >= 10) return Ban(key, TimeSpan.FromMinutes(10), attempts);
            if (attempts >= 5)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, attempts)));
                return (true, delay, attempts);
            }

            return (false, null, attempts);
        }

        private (bool, TimeSpan, int) Ban(string key, TimeSpan duration, int attempts)
        {
            _cache.Set(BanKey(key), true, duration);
            return (true, duration, attempts);
        }
    }
}
