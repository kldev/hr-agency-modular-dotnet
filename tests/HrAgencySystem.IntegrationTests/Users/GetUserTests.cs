using System.Net;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Teams.Contracts;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

/// <summary>
/// The single-user read is what a details screen hangs off, so it is covered on its own rather than
/// only as the read-back step of the update tests.
/// </summary>
[Collection(IntegrationCollection.Name)]
public sealed class GetUserTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTeams();
        await Cleaner.CleanUsers();
    }

    [Fact]
    public async Task ShouldReturnTheUser()
    {
        var user = await UserClient.CreateAsync(
            _organizationId,
            email: "details@test.com",
            firstName: "Ada",
            lastName: "Lovelace",
            role: OrganizationRoleApi.HiringManager
        );

        await Eventually.AssertAsync(
            async () =>
            {
                var result = await UserClient.GetAsync(_organizationId, user.Id);

                Assert.Equal(user.Id, result.Id);
                Assert.Equal(_organizationId, result.OrganizationId);
                Assert.Equal("details@test.com", result.Email);
                Assert.Equal("Ada", result.FirstName);
                Assert.Equal("Lovelace", result.LastName);
                Assert.Equal("Ada Lovelace", result.FullName);
                Assert.Equal(OrganizationRole.HiringManager, result.Role);
            },
            output: OutputHelper
        );
    }

    [Fact]
    public async Task ShouldCarryTheTeamThePersonBelongsTo()
    {
        var team = await TeamClient.CreateAsync(
            _organizationId,
            "Tiggers",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        var user = await UserClient.CreateAsync(
            _organizationId,
            email: "seated@test.com",
            teamId: team.TeamId,
            teamRole: TeamRole.Recruiter
        );

        await Eventually.AssertAsync(
            async () =>
            {
                var result = await UserClient.GetAsync(_organizationId, user.Id);

                Assert.NotNull(result.Team);
                Assert.Equal(team.TeamId, result.Team.Id);
                Assert.Equal("Tiggers", result.Team.Name);
                Assert.Equal(TeamRole.Recruiter, result.Team.Role);
            },
            output: OutputHelper
        );
    }

    [Fact]
    public async Task ShouldReturnNotFoundForAnUnknownUser()
    {
        var response = await UserClient.GetRawAsync(_organizationId, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// A user from another tenant answers 404 rather than 403 — the reply must not reveal that the
    /// id exists somewhere else.
    /// </summary>
    [Fact]
    public async Task ShouldNotReturnAUserFromAnotherOrganization()
    {
        var user = await UserClient.CreateAsync(_organizationId, email: "foreign@test.com");

        var response = await UserClient.GetRawAsync(Guid.NewGuid(), user.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
