
/*
 * ipAddress as nullable/optional because if, for example, another API makes a request to this API / server-to-server communication,
 * i cannot guarantee that the request contains an IP address.
 */

using Application.Common;
using Application.DTOs;

namespace Application.Abstractions;

public interface IAuthenticationService
{
    Task<Result<bool>> CheckEmailAsync(CheckEmailRequest request, CancellationToken ct = default);

    Task<Result<RegisterResult>> RegisterAsync(RegisterAuthRequest request, CancellationToken ct = default);

    Task<Result<LoginResult>> LoginAsync(LoginAuthRequest request, string? ipAddress, CancellationToken ct = default);

    Task<Result<RefreshTokenResult>> RefreshAsync(RefreshAuthRequest request, string? ipAddress, CancellationToken ct = default);

    Task<Result<CurrentUserResult>> GetCurrentUserAsync(string userId, CancellationToken ct = default);

    Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken ct = default);
}
