
// Defines keys used for JWT claims.
// The keys are used as labels for information sent in the JWT payload / response.

namespace Infrastructure.Security;

public static class JwtClaimTypes
{
    public const string UserId = "userId";
    public const string Email = "email";
    public const string Role = "role";
}
