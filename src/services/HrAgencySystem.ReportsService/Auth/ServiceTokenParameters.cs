using System.Text;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.ReportsService.Auth;

/// <summary>
/// What makes a service token acceptable, in one place so a test can ask the same question the
/// middleware asks - the file service's pattern.
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
            ValidIssuer = ReportsServiceToken.Issuer,
            ValidAudience = ReportsServiceToken.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
}
