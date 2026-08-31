namespace HrAgencySystem.Identity.Application.Port;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Matches(string password, string hash);
}