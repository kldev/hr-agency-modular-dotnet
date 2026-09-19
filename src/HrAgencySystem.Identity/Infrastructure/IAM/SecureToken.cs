using System.Security.Cryptography;
using System.Text;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

/// <summary>
/// Random secrets that are mailed or handed to a client and only ever stored as a hash. The reset
/// link and the refresh token are the same mechanism, so they share the same generator - a change
/// of token length or hash here changes both, instead of one of them quietly staying behind.
/// </summary>
public static class SecureToken
{
    public static string New() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
