using System.Security.Claims;
using System.Text;
using HrAgencySystem.ReportsService.Auth;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HrAgencySystem.ReportsService.UnitTests;

public sealed class ServiceTokenTests
{
    private const string Secret = "reports-service-secret-long-enough-for-hmac-sha256";
    private const string OtherSecret = "a-completely-different-secret-of-sufficient-length";

    private readonly JsonWebTokenHandler _handler = new();

    [Fact]
    public async Task AWellFormedTokenIsAccepted()
    {
        var result = await Validate(Token([Organization()]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ATokenSignedWithAnotherSecretIsRejected()
    {
        var result = await Validate(Token([Organization()], secret: OtherSecret));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AFileServiceTokenIsRejected()
    {
        // Correctly signed, but for the other service - one key per audience is the rule.
        var result = await Validate(Token([Organization()], audience: "file-service"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AnExpiredTokenIsRejected()
    {
        var result = await Validate(Token([Organization()], lifetime: TimeSpan.FromMinutes(-10)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AnOrganizationTokenOpensTheOrganizationReportOnly()
    {
        var principal = Principal(Organization());

        Assert.True(await Authorize(principal, ReportPolicies.Organization));
        Assert.False(await Authorize(principal, ReportPolicies.Platform));
    }

    [Fact]
    public async Task APlatformTokenOpensThePlatformReportOnly()
    {
        var principal = Principal(
            new Claim(ReportsServiceToken.ScopeClaim, ReportsServiceToken.PlatformScope)
        );

        Assert.True(await Authorize(principal, ReportPolicies.Platform));
        Assert.False(await Authorize(principal, ReportPolicies.Organization));
    }

    [Fact]
    public async Task AnUnreadableOrganizationClaimOpensNothing()
    {
        var principal = Principal(new Claim(ReportsServiceToken.OrganizationClaim, "not-a-guid"));

        Assert.False(await Authorize(principal, ReportPolicies.Organization));
    }

    private static Claim Organization() =>
        new(ReportsServiceToken.OrganizationClaim, Guid.NewGuid().ToString());

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));

    private static async Task<bool> Authorize(ClaimsPrincipal principal, string policy)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        ReportPolicies.Add(services.AddAuthorizationBuilder());

        var authorization = services
            .BuildServiceProvider()
            .GetRequiredService<IAuthorizationService>();

        return (await authorization.AuthorizeAsync(principal, policy)).Succeeded;
    }

    private string Token(
        Claim[] claims,
        string secret = Secret,
        string audience = ReportsServiceToken.Audience,
        TimeSpan? lifetime = null
    )
    {
        var now = DateTime.UtcNow;
        var expires = now.Add(lifetime ?? ReportsServiceToken.Lifetime);

        return _handler.CreateToken(
            new SecurityTokenDescriptor
            {
                Issuer = ReportsServiceToken.Issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = expires < now ? expires.AddMinutes(-1) : now,
                IssuedAt = expires < now ? expires.AddMinutes(-1) : now,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    SecurityAlgorithms.HmacSha256
                ),
            }
        );
    }

    private Task<TokenValidationResult> Validate(string token) =>
        _handler.ValidateTokenAsync(token, ServiceTokenParameters.Create(Secret));
}
