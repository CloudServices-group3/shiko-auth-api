
/*
 * ipAddress as nullable/optional because if, for example, another API makes a request to this API / server-to-server communication,
 * i cannot guarantee that the request contains an IP address.
 */

using Application.DTOs;

namespace Application.Abstractions;

public interface IRefreshTokenService
{
    Task<string> CreateAsync(string userId, string? ipAddress, CancellationToken ct = default);

    Task<RotateRefreshTokenResult> RotateAsync(string plainToken, string? ipAddress, CancellationToken ct = default);

    Task RevokeAsync(string plainToken, string? ipAddress, CancellationToken ct = default);
}
