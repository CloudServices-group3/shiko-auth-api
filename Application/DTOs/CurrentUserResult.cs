namespace Application.DTOs;

public sealed record CurrentUserResult
(
    string UserId,
    string Email,
    string? PhoneNumber,
    IList<string> Roles
);