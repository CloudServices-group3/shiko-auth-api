namespace Application.DTOs;

public sealed record UserInfoDto
(
    string UserId,
    string Email,
    IList<string> Roles
);
