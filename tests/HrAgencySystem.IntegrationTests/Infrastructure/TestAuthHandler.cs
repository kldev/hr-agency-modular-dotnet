using System.Security.Claims;
using System.Text.Encodings.Web;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

internal class TestAuthHandler(
    IOptionsMonitor<TestAuthHandlerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<TestAuthHandlerOptions>(options, logger, encoder)
{
    internal const string AuthenticationScheme = "TestScheme";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = BuildDefaultClaims(Options);

        UseTestOrganizationId(claims);
        UseTestRoles(claims);
        
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private void UseTestOrganizationId(List<Claim> claims)
    {
        var testOrganizationId = Request.Headers["X-Test-OrganizationId"];
        if (!Guid.TryParse(testOrganizationId, out var organizationId)) return;
        claims.RemoveAll(x => x.Type == AppClaims.OrganizationId);
        claims.Add(new Claim(
            AppClaims.OrganizationId,
            organizationId.ToString()));
    }

    private void UseTestRoles(List<Claim> claims)
    {
        var testRoles = Request.Headers
            .GetCommaSeparatedValues("X-Test-Roles");
        if (testRoles.Length <= 0) return;
        claims.RemoveAll(c => c.Type == AppClaims.Role);
        claims.AddRange(testRoles.Select(role => new Claim(AppClaims.Role, role)));
    }

    private List<Claim> BuildDefaultClaims(TestAuthHandlerOptions options)
    {
        List<Claim> claims =
        [
            new(AppClaims.UserId, Options.Id.ToString()),
            new(AppClaims.Email, TestAuthHandlerOptions.Email),
            new(AppClaims.FullName, "Test user/owner"),
            new(AppClaims.OrganizationId, Options.OrganizationId.ToString()),
            .. Options.Roles.Select(role => new Claim(AppClaims.Role, role)),
        ];
        return claims;
    }
}

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public Guid Id { get; } = Guid.NewGuid();
    public static string Email => "test@example.com";
    public string[] Roles { get; } = [nameof(OrganizationRole.Admin), nameof(PlatformRole.Owner)];
    public Guid OrganizationId { get; } = Guid.NewGuid();
}