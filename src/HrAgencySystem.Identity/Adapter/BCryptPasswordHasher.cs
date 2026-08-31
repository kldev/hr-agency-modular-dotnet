using HrAgencySystem.Identity.Application.Port;

namespace HrAgencySystem.Identity.Adapter;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Matches(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}