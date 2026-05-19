
// Using: Microsoft.EntityFrameworkCore.InMemory for the in-memory database.

using Application.Abstractions;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure
        (   
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment
        )
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Use in-memory database in Development environment.
        if (environment.IsDevelopment()) 
        {
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("DevelopmentDb")
            );
        }
        else
        {
            // Use SQL Server in Production environment.
            // Read the Production database connection string from Azure environment variables.
            var connectionString = configuration.GetConnectionString("ProductionDatabaseUrl");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Production database connection string is not set in Azure");

            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connectionString)
            );
        }

        return services;
    }
}
