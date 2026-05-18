
// CORS configuration to define which external origins are allowed to access the API.

using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Security;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins("http://127.0.0.1:5500", "http://localhost:3000", "https://localhost:3000", "https://shiko-auth-api.azurewebsites.net")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
