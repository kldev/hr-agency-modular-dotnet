using System.IdentityModel.Tokens.Jwt;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.UnitTests.Identity.Infrastructure;

public sealed class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void GenerateUserToken_ExpiresAfterTheConfiguredNumberOfHours(int hours)
    {
        var token = Service(hours).GenerateUserToken(User());

        var written = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);

        Assert.Equal(Now.AddHours(hours), token.ExpiresAt);
        Assert.Equal(Now.AddHours(hours).UtcDateTime, written.ValidTo);
    }

    [Fact]
    public void GenerateUserToken_CarriesTheClaimsTheApiBindsOn()
    {
        var user = User();

        var token = Service().GenerateUserToken(user);

        var claims = new JwtSecurityTokenHandler()
            .ReadJwtToken(token.Value)
            .Claims.ToDictionary(x => x.Type, x => x.Value);

        Assert.Equal(user.Id.ToString(), claims[AppClaims.UserId]);
        Assert.Equal(user.Email, claims[AppClaims.Email]);
        Assert.Equal(user.OrganizationId.ToString(), claims[AppClaims.OrganizationId]);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(30)]
    [InlineData(120)]
    public void GenerateImpersonationToken_ExpiresAfterTheConfiguredNumberOfMinutes(int minutes)
    {
        var token = Service(impersonationExpiresInMinutes: minutes)
            .GenerateImpersonationToken(User(), Guid.NewGuid());

        var written = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);

        Assert.Equal(Now.AddMinutes(minutes), token.ExpiresAt);
        Assert.Equal(Now.AddMinutes(minutes).UtcDateTime, written.ValidTo);
    }

    /// <summary>
    /// The point of the whole feature: authorization downstream sees the target and nothing else, so
    /// an administrator standing in for somebody gets exactly what that person gets - no more, and
    /// no read-only half measure either. The extra claim only says who is standing in.
    /// </summary>
    [Fact]
    public void GenerateImpersonationToken_IsTheTargetsTokenPlusTheAdministratorsName()
    {
        var user = User();
        var admin = Guid.NewGuid();

        var token = Service().GenerateImpersonationToken(user, admin);

        var claims = new JwtSecurityTokenHandler()
            .ReadJwtToken(token.Value)
            .Claims.ToDictionary(x => x.Type, x => x.Value);

        Assert.Equal(user.Id.ToString(), claims[AppClaims.UserId]);
        Assert.Equal(user.OrganizationId.ToString(), claims[AppClaims.OrganizationId]);
        Assert.Equal(user.Role.ToString(), claims[AppClaims.Role]);
        Assert.Equal(admin.ToString(), claims[AppClaims.ImpersonatedBy]);
    }

    [Fact]
    public void GenerateUserToken_SaysNobodyIsStandingInForTheUser()
    {
        var token = Service().GenerateUserToken(User());

        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token.Value).Claims;

        Assert.DoesNotContain(claims, claim => claim.Type == AppClaims.ImpersonatedBy);
    }

    private static JwtTokenService Service(
        int expiresInHours = 6,
        int impersonationExpiresInMinutes = 30
    ) =>
        new(
            Options.Create(
                new JwtConfig
                {
                    Issuer = "hr-agency-api",
                    Audience = "hr-agency",
                    SecretKey = "SuperSecretKeyForDemoPurposesOnly-AtLeast32Bytes!",
                    ExpiresInHours = expiresInHours,
                    ImpersonationExpiresInMinutes = impersonationExpiresInMinutes,
                }
            ),
            new FixedClock(Now)
        );

    private static UserProjection User()
    {
        var organizationId = Guid.NewGuid();

        return new UserProjection(
            Guid.NewGuid(),
            organizationId,
            "bob.smith@hr-agency.com",
            "Bob",
            "Smith",
            OrganizationRole.Recruiter,
            Guid.NewGuid(),
            new UserSnapshot(Guid.NewGuid(), "Ann", "Boss", "ann.boss@hr-agency.com"),
            Now,
            new OrganizationInfo(organizationId, "hr-agency", "HR Agency")
        );
    }
}
