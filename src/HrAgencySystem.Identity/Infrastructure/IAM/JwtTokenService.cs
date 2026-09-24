using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public sealed class JwtTokenService(IOptions<JwtConfig> configuration, IClock clock)
    : IJwtTokenService
{
    public AccessToken GenerateUserToken(UserProjection user)
    {
        Claim[] claims =
        [
            new(AppClaims.UserId, user.Id.ToString()),
            new(AppClaims.Email, user.Email),
            new(AppClaims.Role, user.Role.ToString()),
            new(AppClaims.OrganizationId, user.OrganizationId.ToString()),
            new(AppClaims.FullName, user.FullName),
        ];

        return CreateToken(claims);
    }

    public AccessToken GenerateOwnerToken(OwnerProjection owner)
    {
        Claim[] claims =
        [
            new(AppClaims.UserId, owner.Id.ToString()),
            new(AppClaims.Email, owner.Email),
            new(AppClaims.Role, owner.Role.ToString()),
        ];

        return CreateToken(claims);
    }

    public AccessToken GenerateImpersonationToken(UserProjection user, Guid impersonatedBy)
    {
        Claim[] claims =
        [
            new(AppClaims.UserId, user.Id.ToString()),
            new(AppClaims.Email, user.Email),
            new(AppClaims.Role, user.Role.ToString()),
            new(AppClaims.OrganizationId, user.OrganizationId.ToString()),
            new(AppClaims.FullName, user.FullName),
            new(AppClaims.ImpersonatedBy, impersonatedBy.ToString()),
        ];

        return CreateToken(
            claims,
            TimeSpan.FromMinutes(configuration.Value.ImpersonationExpiresInMinutes)
        );
    }

    /// <param name="claims">The claims the token carries.</param>
    /// <param name="lifetime">
    /// Overrides the configured access token window. Only impersonation passes one - it needs a
    /// shorter leash than a login, and nothing is going to refresh it.
    /// </param>
    private AccessToken CreateToken(Claim[] claims, TimeSpan? lifetime = null)
    {
        var config = configuration.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));

        var expires = clock.UtcNow.Add(lifetime ?? TimeSpan.FromHours(config.ExpiresInHours));
        var token = new JwtSecurityToken(
            issuer: config.Issuer,
            audience: config.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(tokenValue, expires);
    }
}
