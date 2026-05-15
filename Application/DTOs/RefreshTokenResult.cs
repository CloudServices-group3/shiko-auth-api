namespace Application.DTOs;

public sealed record RefreshTokenResult
(
    string AccessToken,
    string TokenType,
    int Expires,
    DateTimeOffset ExpiresAtUtc,
    string RefreshToken,
    UserInfoDto UserInfo
);
