
// JwtOptions to configure how to generate and validating JWT tokens.

using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SigningKey { get; set; } = null!;
    public int AccessTokenMinutes { get; set; } = 10;
    public int RefreshTokenDays { get; set; } = 30;


    /*
     * Method to convert the SigningKey into a Byte array and pack it into a SymmetricSecurityKey used for JWT signing.
     */
    public SymmetricSecurityKey GetSigningKey()
    {
        var bytes = Convert.FromBase64String(SigningKey);

        return new SymmetricSecurityKey(bytes);
    }
}
