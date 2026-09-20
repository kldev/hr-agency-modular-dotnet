using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HrAgencySystem.FileService.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

/// <summary>
/// Mints a token for one call. It carries the organization as a signed claim so the file service
/// never has to take the caller's word for which tenant it is acting as.
/// </summary>
public sealed class FileServiceTokenFactory(IOptions<FileServiceClientConfig> options)
{
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret)),
        SecurityAlgorithms.HmacSha256
    );

    public string Create(Guid organizationId, Guid actorId)
    {
        var token = new JwtSecurityToken(
            issuer: FileServiceToken.Issuer,
            audience: FileServiceToken.Audience,
            claims:
            [
                new Claim(FileServiceToken.OrganizationClaim, organizationId.ToString()),
                new Claim(FileServiceToken.ActorClaim, actorId.ToString()),
            ],
            expires: DateTime.UtcNow.Add(FileServiceToken.Lifetime),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
