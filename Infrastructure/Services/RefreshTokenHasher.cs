using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class RefreshTokenHasher
{
    // If you hash the exact same token on two different occasions, the hash value is the same and this way you can compare the input data with what is in the database.

    public string Hash(string token)
    {
        // Hashes the input token using SHA-256 and converts the result to Base64 so it can be safely stored and compared as a string.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
