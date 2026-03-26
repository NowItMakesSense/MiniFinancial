using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Infrastructure.Persistence.Contracts.Services;
using MiniFinancial.Infrastructure.Persistence.Repositories;

namespace MiniFinancial.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration["Database:Provider"];
            var connectionString = configuration.GetConnectionString("MiniFinancialLedgerKey");

            services.AddDbContext<AppDbContext>(options =>
            {
                switch (provider)
                {
                    case "SqlServer":
                        options.UseSqlServer(connectionString);
                        break;

                    default:
                        throw new InvalidOperationException("Provider não suportado.");
                }
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();

            services.AddScoped<IApplicationDbContext, AppDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
