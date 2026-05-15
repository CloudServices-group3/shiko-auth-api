using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Infrastructure.Services;


// This service is responsible for creating, rotating and revoking a refresh token.

public class RefreshTokenService(DataContext context, RefreshTokenHasher hasher, IOptions<JwtOptions> options) : IRefreshTokenService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<string> CreateAsync(string userId, string? ipAddress, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var plainToken = GenerateToken();
        var refreshToken = new RefreshToken   // Creates a new database entity with a refresh token for the user session.
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hasher.Hash(plainToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays),
            CreatedByIp = ipAddress,
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);

        return plainToken;
    }

    public async Task<RotateRefreshTokenResult> RotateAsync(string plainToken, string? ipAddress, CancellationToken ct = default)
    {
        // Hashes the input token to be compared with the token in the database.
        var tokenHash = hasher.Hash(plainToken);

        // Compares the input Token with the token in the database.
        var currentToken = await context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (currentToken == null || !currentToken.IsActive)
        {
            return RotateRefreshTokenResult.Failed();
        }

        var newPlainToken = GenerateToken();

        var now = DateTime.UtcNow;

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = currentToken.UserId,
            TokenHash = hasher.Hash(newPlainToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays),
            CreatedByIp = ipAddress,
        };

        currentToken.RevokedAtUtc = now;
        currentToken.RevokedByIp = ipAddress;
        currentToken.ReplacedByTokenId = newRefreshToken.Id;
        
        // Adds the new token to database.
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync(ct);

        return RotateRefreshTokenResult.Success(currentToken.UserId, newPlainToken);
    }

    public async Task RevokeAsync(string plainToken, string? ipAddress, CancellationToken ct = default)
    {
        // Hashes the input token to be compared with the token in the database.
        var tokenHash = hasher.Hash(plainToken);

        // Gets the current refresh token from the database.
        var token = await context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (token == null || !token.IsActive)
            return;

        // Marks the refresh token as revoked so it can no longer be used.
        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;

        await context.SaveChangesAsync(ct);
    }

    public static string GenerateToken()
    {
        // Creates a new refresh token.
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes); // Converts a byte array with binary data to a Base64 string.
    }
}
