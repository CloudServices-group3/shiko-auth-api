namespace Application.DTOs;

public sealed record LoginResult
(
    AccessTokenResult AccessToken,
    string RefreshToken,
    UserInfoDto UserInfo
);
