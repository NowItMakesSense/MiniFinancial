using MiniFinancial.Application.Contracts.Services;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Presentation.Middlewares
{
    public class AbuseMiddleware
    {
        private readonly RequestDelegate _next;

        public AbuseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AbuseProtectionService abuse)
        {
            var ip = GetClientIp(context);
            var key = ip;

            if (abuse.IsBanned(key))
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("IP temporariamente bloqueado.");
                return;
            }

            try
            {
                await _next(context);
            }
            catch (OwnershipViolationException)
            {
                var (blocked, retryAfter, attempts) = abuse.RegisterFailure(key);
                context.Response.StatusCode = blocked ? 429 : 403;

                if (retryAfter.HasValue) context.Response.Headers["Retry-After"] = ((int)retryAfter.Value.TotalSeconds).ToString();

                await context.Response.WriteAsync("Acesso negado.");
            }
        }

        private static string GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                var ip = forwarded.ToString().Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                    return ip;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
