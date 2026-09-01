using Microsoft.Extensions.Options;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public class JwtConfig
{
    public const string Section = "Jwt";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public int ExpiresInHours { get; set; } = 12;
}