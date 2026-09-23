using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

/// <summary>
/// Mints a token for one call. An organization report's token names the organization and nothing
/// wider; the platform report's carries the platform scope and no organization - so the service can
/// tell the two apart without trusting a single parameter.
/// </summary>
public sealed class ReportsTokenFactory(IOptions<ReportsClientConfig> options)
{
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret)),
        SecurityAlgorithms.HmacSha256
    );

    public string ForOrganization(Guid organizationId, Guid actorId) =>
        Create(
            new Claim(ReportsServiceToken.OrganizationClaim, organizationId.ToString()),
            new Claim(ReportsServiceToken.ActorClaim, actorId.ToString())
        );

    public string ForPlatform(Guid ownerId) =>
        Create(
            new Claim(ReportsServiceToken.ScopeClaim, ReportsServiceToken.PlatformScope),
            new Claim(ReportsServiceToken.ActorClaim, ownerId.ToString())
        );

    private string Create(params Claim[] claims)
    {
        var token = new JwtSecurityToken(
            issuer: ReportsServiceToken.Issuer,
            audience: ReportsServiceToken.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(ReportsServiceToken.Lifetime),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
