
using System.Security.Cryptography;
namespace notion_clone.Extentions;

public static class GuidExtensions
{
    public static Guid GenerateSecureGuid(this Guid guid)
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[16];  // Size of a UUID.
        rng.GetBytes(randomBytes);

        return new Guid(randomBytes);
    }
}
