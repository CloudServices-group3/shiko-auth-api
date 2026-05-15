using Application.DTOs;

namespace Application.Abstractions;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(string userId, string email, IList<string> roles);
}
