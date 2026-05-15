namespace Application.DTOs;

public sealed record AccessTokenResult
(
    string AccessToken,
    string TokenType,
    int Expires,        // Lifetime of the access token in seconds. ( Used by client for token refresh logic ) 
    DateTimeOffset ExpiresAtUtc
);
