using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Common.Behaviors;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Services;

namespace MiniFinancial.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            services.AddMemoryCache();
            services.AddScoped<AbuseProtectionService>();

            return services;
        }
    }
}
