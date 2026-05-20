using Application.Abstractions;
using Application.DTOs;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace Shiko.Auth.Api.Endpoints;

public static class AuthenticationEndpoints
{

    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/check-email", CheckEmail);
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout);
        group.MapGet("/me", Me).RequireAuthorization(); // An endpoint for the frontend to be able to find out who is logged in.
        group.MapDelete("/user/{userId}", DeleteUser);
    }

    private static async Task<IResult> CheckEmail(CheckEmailRequest request, IAuthenticationService authenticationService, CancellationToken ct = default)
    {
        if (request is null)
            return Results.BadRequest("Request cannot be empty");

        var result = await authenticationService.CheckEmailAsync(request, ct);

        if (!result.IsSuccess)
            return Results.BadRequest();

        return Results.Ok(new { exists = result.Value });
    }

    private static async Task<IResult> Register(RegisterAuthRequest request, IAuthenticationService authenticationService, CancellationToken ct = default)
    {
        var result = await authenticationService.RegisterAsync(request, ct);

        if (!result.IsSuccess)
            return Results.ValidationProblem(result.errors ?? new Dictionary<string, string[]>());

        return Results.Created($"/api/users/{result.Value!.UserId}", result.Value);
    }

    private static async Task<IResult> Login (LoginAuthRequest request, IAuthenticationService authenticationService,HttpContext httpContext, CancellationToken ct = default)
    {
        // HttpContext = all information about the request + user + connection and metadata.

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authenticationService.LoginAsync(request, ipAddress, ct);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Refresh(RefreshAuthRequest request,IAuthenticationService authenticationService, HttpContext httpContext, CancellationToken ct = default)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authenticationService.RefreshAsync(request, ipAddress, ct);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Logout(LogoutAuthRequest request, RefreshTokenService refreshTokenService, HttpContext httpContext, CancellationToken ct = default)
    {
        // Disables/invalidates the refresh token so that it is no longer valid.
        // We revoke refresh tokens on logout to prevent reuse if the token is stolen.
        await refreshTokenService.RevokeAsync(request.RefreshToken, httpContext.Connection.RemoteIpAddress?.ToString(), ct);

        return Results.NoContent();
    }

    [Authorize]
    private static async Task<IResult> Me(HttpContext httpContext, IAuthenticationService authenticationService, CancellationToken ct = default)
    {
        // HttpContext = all information about the request + user + connection and metadata.

        var userId = httpContext.User.FindFirst(JwtClaimTypes.UserId)?.Value;

        if (userId == null)
            return Results.NoContent();

        var result = await authenticationService.GetCurrentUserAsync(userId, ct);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteUser(string userId, IAuthenticationService authenticationService, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Results.BadRequest("User ID is required");

        var result = await authenticationService.DeleteUserAsync(userId, ct);

        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.NoContent();
    }
}
