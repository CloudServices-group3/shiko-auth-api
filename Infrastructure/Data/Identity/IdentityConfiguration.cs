using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data.Identity;

public static class IdentityConfiguration
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<AppUser>(options =>
        { 
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;

            // Lockout configuration used as protection against brute-force attacks by limiting failed login attempts and temporarily blocking the account.
            options.Lockout.MaxFailedAccessAttempts = 5; 
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders(); // This is not the same as JWT. This is something that identity uses when you want to change something in your data and then identity sends a verification link.

        return services;
    }
}
