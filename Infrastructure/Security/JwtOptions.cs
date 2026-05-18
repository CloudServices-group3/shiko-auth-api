
// JwtOptions to configure how to generate and validating JWT tokens.

using Microsoft.IdentityModel.Tokens;
using System.Text;

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
     * Method to convert the SigningKey into a UTF-8 byte array and pack it into a SymmetricSecurityKey that is used for SHA-256 JWT signing and validation.
     */
    public SymmetricSecurityKey GetSigningKey()
    {
        var bytes = Encoding.UTF8.GetBytes(SigningKey);

        return new SymmetricSecurityKey(bytes);
    }
}
