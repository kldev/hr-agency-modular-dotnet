using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Projections;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public sealed class JwtTokenService(IOptions<JwtConfig> configuration) : IJwtTokenService
{
    public string GenerateUserToken(UserProjection user)
    {
        Claim[] claims =
        [
            new (AppClaims.UserId, user.Id.ToString()),
            new (AppClaims.Email, user.Email),
            new (AppClaims.Role, user.Role.ToString()),
            new (AppClaims.OrganizationId, user.OrganizationId.ToString())
        ];

        return CreateToken(claims);
    }

    public string GenerateOwnerToken(OwnerProjection owner)
    {
        Claim[] claims =
        [
            new (AppClaims.UserId, owner.Id.ToString()),
            new (AppClaims.Email, owner.Email),
            new (AppClaims.Role, owner.Role.ToString()),
        ];

        return CreateToken(claims);
    }
    
    private string CreateToken(Claim[] claims)
    {
        var config = configuration.Value;
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config.SecretKey));
        
        var expires = DateTime.UtcNow.AddHours(6);
        var token = new JwtSecurityToken(
            issuer: config.Issuer,
            audience: config.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;

    }
}