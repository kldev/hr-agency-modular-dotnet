namespace HrAgencySystem.Identity.Infrastructure.IAM;

public class JwtConfig
{
    public const string Section = "Jwt";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SecretKey { get; set; } = "";

    /// How long an access token stays valid. Short on purpose: it cannot be revoked, so the refresh
    /// token is what carries the session and this is only the window an intercepted token buys.
    public int ExpiresInHours { get; set; } = 6;

    /// How long a refresh token stays valid, counted from the login. Rotation inherits the date, so
    /// this is the whole session length - after it the user signs in again.
    public int RefreshTokenExpiresInDays { get; set; } = 30;
}
