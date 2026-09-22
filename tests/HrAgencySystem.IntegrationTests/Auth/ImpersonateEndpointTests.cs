using System.IdentityModel.Tokens.Jwt;
using System.Net;
using HrAgencySystem.Identity.Application.Users.Impersonate;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Auth;

[Collection(IntegrationCollection.Name)]
public sealed class ImpersonateEndpointTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper
) : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanUsers();
    }

    /// <summary>
    /// The token is the target's in everything authorization reads - their id, their organization,
    /// their role - and says who is standing in for them on top of that.
    /// </summary>
    [Fact]
    public async Task ShouldIssueATokenThatIsTheTargetsPlusTheAdministratorsName()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(
            organizationId,
            "target@test.com",
            OrganizationRoleApi.Recruiter
        );

        var result = await Impersonate(organizationId, admin, target);

        Assert.Equal(target, result.ImpersonatedUserId);
        Assert.Equal(admin, result.ImpersonatedBy);

        var claims = Claims(result.Token);

        Assert.Equal(target.ToString(), claims[AppClaims.UserId]);
        Assert.Equal(organizationId.ToString(), claims[AppClaims.OrganizationId]);
        Assert.Equal(nameof(OrganizationRole.Recruiter), claims[AppClaims.Role]);
        Assert.Equal(admin.ToString(), claims[AppClaims.ImpersonatedBy]);
    }

    /// <summary>
    /// Thirty minutes rather than the six hours a login buys, and nothing to refresh afterwards -
    /// the shape of the answer is what guarantees the second half.
    /// </summary>
    [Fact]
    public async Task ShouldIssueAShortLivedTokenWithNothingToRefresh()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(organizationId, "target@test.com");

        var response = await Send(organizationId, admin, target);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("refreshToken", body, StringComparison.OrdinalIgnoreCase);

        var result = await response.ReadWithJson<ImpersonationResult>(OutputHelper);

        Assert.NotNull(result);
        Assert.InRange(
            result.ExpiresAt,
            DateTimeOffset.UtcNow.AddMinutes(25),
            DateTimeOffset.UtcNow.AddMinutes(35)
        );
    }

    [Fact]
    public async Task ShouldRefuseToSignInAsYourself()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");

        var response = await Send(organizationId, admin, admin);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Allowed on purpose. Refusing would guard nothing: an administrator can lower another one's
    /// role, sign in as them and put it back.
    /// </summary>
    [Fact]
    public async Task ShouldAllowSigningInAsAnotherAdministrator()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var other = await CreateUser(organizationId, "other.admin@test.com");

        var result = await Impersonate(organizationId, admin, other);

        Assert.Equal(other, result.ImpersonatedUserId);
    }

    /// <summary>
    /// Not found rather than forbidden: a refusal would tell the caller the id exists somewhere.
    /// </summary>
    [Fact]
    public async Task ShouldAnswerNotFoundForSomebodyInAnotherOrganization()
    {
        var organizationId = Guid.NewGuid();
        var elsewhere = Guid.NewGuid();

        var admin = await CreateUser(organizationId, "admin@test.com");
        var stranger = await CreateUser(elsewhere, "stranger@test.com");

        var response = await Send(organizationId, admin, stranger);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldAnswerNotFoundForSomebodyWhoDoesNotExist()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");

        var response = await Send(organizationId, admin, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// The whole reason the user management endpoints were closed at the same time: without this,
    /// anybody could hand themselves the role that opens this door.
    /// </summary>
    [Fact]
    public async Task ShouldRefuseACallerWhoIsNotAnAdministrator()
    {
        var organizationId = Guid.NewGuid();
        var recruiter = await CreateUser(organizationId, "recruiter@test.com");
        var target = await CreateUser(organizationId, "target@test.com");

        var client = Env.CreateClient();

        client.SetTestRoles(nameof(OrganizationRole.Recruiter));
        client.WithOrganizationId(organizationId);
        client.WithUserId(recruiter);

        var response = await client.PostAsync($"/api/auth/impersonate/{target}", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<ImpersonationResult> Impersonate(
        Guid organizationId,
        Guid adminId,
        Guid targetId
    )
    {
        var response = await Send(organizationId, adminId, targetId);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<ImpersonationResult>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    private async Task<HttpResponseMessage> Send(Guid organizationId, Guid adminId, Guid targetId)
    {
        Client.WithOrganizationId(organizationId);
        Client.WithUserId(adminId);

        return await Client.PostAsync($"/api/auth/impersonate/{targetId}", null);
    }

    private static Dictionary<string, string> Claims(string token) =>
        new JwtSecurityTokenHandler()
            .ReadJwtToken(token)
            .Claims.ToDictionary(claim => claim.Type, claim => claim.Value);

    /// <summary>
    /// Waits out the user read model: impersonation reads the target from it, so without this the
    /// tests would be measuring the projection daemon rather than the feature.
    /// </summary>
    private async Task<Guid> CreateUser(
        Guid organizationId,
        string email,
        OrganizationRoleApi role = OrganizationRoleApi.Admin
    )
    {
        var user = await UserClient.CreateAsync(organizationId, email, "John", "Doe", role);

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);
            Client.WithUserId(user.Id);

            var response = await Client.GetAsync("/api/users/me");

            response.EnsureSuccessStatusCode();
        });

        return user.Id;
    }
}
