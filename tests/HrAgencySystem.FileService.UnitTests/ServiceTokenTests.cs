using System.Security.Claims;
using System.Text;
using HrAgencySystem.FileService.Auth;
using HrAgencySystem.FileService.Contracts;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.FileService.UnitTests;

public sealed class ServiceTokenTests
{
    private const string Secret = "file-service-secret-long-enough-for-hmac-sha256";
    private const string OtherSecret = "a-completely-different-secret-of-sufficient-length";

    private static readonly Guid OrganizationId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );
    private static readonly Guid ActorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly JsonWebTokenHandler _handler = new();

    [Fact]
    public async Task AWellFormedServiceTokenIsAccepted()
    {
        var result = await Validate(Token());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ATokenSignedWithAnotherSecretIsRejected()
    {
        var result = await Validate(Token(secret: OtherSecret));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AnExpiredTokenIsRejected()
    {
        // Well past the thirty second skew the parameters allow.
        var result = await Validate(Token(lifetime: TimeSpan.FromMinutes(-10)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ATokenMeantForAnotherAudienceIsRejected()
    {
        // This is what a user's API token looks like from here: correctly signed for something else.
        var result = await Validate(Token(audience: "hr-api"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ATokenFromAnotherIssuerIsRejected()
    {
        var result = await Validate(Token(issuer: "somebody-else"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ACallerWithoutAnOrganizationClaimIsRefused()
    {
        var principal = Principal(
            new Claim(FileServiceToken.ActorClaim, ActorId.ToString())
        );

        Assert.Throws<UnauthorizedAccessException>(() => Bind(principal));
    }

    [Fact]
    public void ACallerWithAnUnparsableOrganizationClaimIsRefused()
    {
        var principal = Principal(new Claim(FileServiceToken.OrganizationClaim, "not-a-guid"));

        Assert.Throws<UnauthorizedAccessException>(() => Bind(principal));
    }

    [Fact]
    public void AMissingActorIsToleratedBecauseItOnlyStampsTheRecord()
    {
        var principal = Principal(
            new Claim(FileServiceToken.OrganizationClaim, OrganizationId.ToString())
        );

        var caller = Bind(principal);

        Assert.Equal(OrganizationId, caller.OrganizationId);
        Assert.Equal(Guid.Empty, caller.ActorId);
    }

    private Task<TokenValidationResult> Validate(string token) =>
        _handler.ValidateTokenAsync(token, ServiceTokenParameters.Create(Secret));

    private string Token(
        string secret = Secret,
        string issuer = FileServiceToken.Issuer,
        string audience = FileServiceToken.Audience,
        TimeSpan? lifetime = null
    )
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256
        );

        return _handler.CreateToken(
            new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.Add(lifetime ?? FileServiceToken.Lifetime),
                SigningCredentials = credentials,
                Claims = new Dictionary<string, object>
                {
                    [FileServiceToken.OrganizationClaim] = OrganizationId.ToString(),
                    [FileServiceToken.ActorClaim] = ActorId.ToString(),
                },
            }
        );
    }

    private static ServiceCaller Bind(ClaimsPrincipal principal)
    {
        var context = new DefaultHttpContext { User = principal };

        return ServiceCaller.BindAsync(context, null!).Result!;
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));
}
