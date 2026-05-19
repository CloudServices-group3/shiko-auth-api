using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Identity;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Shiko.Auth.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration(); // File for configuring CORS policies and allowed origins.

builder.Services.AddIdentityConfiguration(builder.Configuration); // File in the Infrastructure layer with configuration for JWT and more.

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddInfrastructure
    (
        builder.Configuration,
        builder.Environment    
    );

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("Frontend"); // Enable CORS using the "Frontend" policy.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title= "Authentication API";
        options.Theme = ScalarTheme.Default;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

var conn = builder.Configuration.GetConnectionString("ProductionDatabaseUrl");
Console.WriteLine("AZURE DB = " + conn);

app.UseHttpsRedirection();

app.UseAuthentication(); // Authentication
app.UseAuthorization();

app.MapAuthenticationEndpoints();

app.Run();
