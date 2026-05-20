using Application.Abstractions;
using Application.Common;
using Application.DTOs;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class AuthenticationService(UserManager<AppUser> userManager, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService) : IAuthenticationService
{
    public async Task<Result<CheckEmailResult>> CheckEmailAsync(CheckEmailRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<CheckEmailResult>.Ok
                (
                    new CheckEmailResult(AuthCheckStatus.NotFound)
                );
        }

        return Result<CheckEmailResult>.Ok(
                    new CheckEmailResult(AuthCheckStatus.ReadyForLogin)
            );
    }

    public async Task<Result<RegisterResult>> RegisterAsync(RegisterAuthRequest request, CancellationToken ct = default)
    {
        // ToLowerInvariant() = Converts uppercase letters to lowercase letters without using language-specific rules from the user's computer.
        var email = request.Email.Trim().ToLowerInvariant();

        var user = new AppUser
        {
            UserName = email,
            Email = email,
        };

        // Creating a new user using ASP.NET Core Identity.
        // Includes built-in validation for email format, password rules etc.
        var result = await userManager.CreateAsync(user, request.Password);


        // If User is not created the result contains an Error message from ASP.NET Identity.
        // in ToDictionary we decide what should be the key and value to show frontend. ( Key-value pairs ).
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description });
            return Result<RegisterResult>.Fail(errors);
        }

        return Result<RegisterResult>.Ok(new RegisterResult(user.Id, user.Email));
    }

    public async Task<Result<LoginResult>> LoginAsync(LoginAuthRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<LoginResult>.Fail("Unauthorized");

        if (await userManager.IsLockedOutAsync(user))
            return Result<LoginResult>.Fail("User is temporarily locked out");

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user);
            return Result<LoginResult>.Fail("Unauthorized");
        }

        // The incorrect login counter (part of ASP.NET Core Identity) is reset.
        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);

        if (user.Email is null)
            return Result<LoginResult>.Fail("User Email Address cannot be empty");

        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Email, roles); // Create Access token

        var refreshToken = await refreshTokenService.CreateAsync(user.Id, ipAddress, ct); // Create refresh token.

        var loginResult = new LoginResult
            (
                new AccessTokenResult
                (
                    accessToken.AccessToken,
                    accessToken.TokenType,
                    accessToken.Expires,
                    accessToken.ExpiresAtUtc
                ),
                    refreshToken,
                    new UserInfoDto
                        (
                            user.Id,
                            user.Email,
                            roles.ToList()
                        )
            );

        return Result<LoginResult>.Ok(loginResult);
    }

    public async Task<Result<RefreshTokenResult>> RefreshAsync(RefreshAuthRequest request, string? ipAddress, CancellationToken ct = default)
    {
        // Checks that the refresh token is valid, revokes it and creates a new refresh token.
        var result = await refreshTokenService.RotateAsync(request.RefreshToken, ipAddress, ct);

        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.UserId))
            return Result<RefreshTokenResult>.Fail("Invalid refresh token.");

        if (string.IsNullOrWhiteSpace(result.NewRefreshToken))
            return Result<RefreshTokenResult>.Fail("Refresh token cannot be empty.");

        var user = await userManager.FindByIdAsync(result.UserId);
        if (user is null)
            return Result<RefreshTokenResult>.Fail("User not found");

        var roles = await userManager.GetRolesAsync(user);

        if (user.Email is null)
            return Result<RefreshTokenResult>.Fail("User Email not found.");

        // Creates a new Access token.
        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Email, roles);

        var refreshTokenResult = new RefreshTokenResult
            (
                AccessToken: accessToken.AccessToken,
                TokenType: accessToken.TokenType,
                Expires: accessToken.Expires,
                ExpiresAtUtc: accessToken.ExpiresAtUtc,
                RefreshToken: result.NewRefreshToken,
                UserInfo: new UserInfoDto
                    (
                        user.Id,
                        user.Email,
                        roles.ToList()
                    )
            );

        // Returns a new Access token and a new refresh token.
        return Result<RefreshTokenResult>.Ok(refreshTokenResult);
    }

    public async Task<Result<CurrentUserResult>> GetCurrentUserAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<CurrentUserResult>.Fail("Unauthorized");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<CurrentUserResult>.Fail("Unauthorized");

        var roles = await userManager.GetRolesAsync(user);

        if (string.IsNullOrWhiteSpace(user.Email))
            return Result<CurrentUserResult>.Fail("User Email not found.");

        return Result<CurrentUserResult>.Ok(new CurrentUserResult
            (
                user.Id,
                user.Email,
                user.PhoneNumber,
                roles.ToList()
            ));
    }

    public async Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result<bool>.Fail("User not found");

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return Result<bool>.Fail("Could not delete user");

        return Result<bool>.Ok(true);
    }
}
