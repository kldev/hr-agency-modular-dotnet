using System.Text;
using HrAgencySystem.FileService.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.FileService.Auth;

/// <summary>
/// What makes a service token acceptable, in one place so a test can ask the same question the
/// middleware asks. Building these inline in the bearer options would make the security control the
/// one thing in this service nothing can check.
/// </summary>
public static class ServiceTokenParameters
{
    public static TokenValidationParameters Create(string secret) =>
        new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = FileServiceToken.Issuer,
            ValidAudience = FileServiceToken.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
}
