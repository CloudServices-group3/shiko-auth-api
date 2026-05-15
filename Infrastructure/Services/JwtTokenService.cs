using Application.Abstractions;
using Application.DTOs;
using Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Services;

public class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenResult CreateAccessToken(string userId, string email, IList<string> roles)
    {
        var now = DateTime.UtcNow; // The start of a valid token.
        var expiresAtUtc = now.AddMinutes(_options.AccessTokenMinutes); // When a token expires.

        // Creates a new claim to be sent with the JWT payload.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtClaimTypes.UserId, userId),
            new(JwtClaimTypes.Email, email ?? string.Empty)
        };

        // Adding roles to the claim.
        claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

        // Creates a SigningKey that are used to sign the JWT.
        var credentials = new SigningCredentials(_options.GetSigningKey(), SecurityAlgorithms.HmacSha256);

        // Creates the actual JWT.
        var token = new JwtSecurityToken
            (
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAtUtc,
                signingCredentials: credentials
            );

        // Makes the JWT a "readable string" that is sent to the frontend.
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessTokenResult
            (
                AccessToken: accessToken,
                TokenType: "Bearer",   // Indicates that the token should be sent in the Authorization header as a Bearer token.
                Expires: (int)(expiresAtUtc - now).TotalSeconds,
                ExpiresAtUtc: expiresAtUtc
            );
    }
}
