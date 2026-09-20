using System.Net;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Teams.Contracts;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public sealed class UserTeamTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTeams();
        await Cleaner.CleanUsers();
    }

    [Fact]
    public async Task ShouldSeatANewUserOnTheTeamAndShowItOnBothSides()
    {
        var founder = Guid.NewGuid();
        var team = await TeamClient.CreateAsync(
            _organizationId,
            "Tiggers",
            (founder, TeamRole.Sales)
        );

        var user = await UserClient.CreateAsync(
            _organizationId,
            "seated@test.com",
            teamId: team.TeamId,
            teamRole: TeamRole.Recruiter
        );

        // The event carries the team, so the 201 body already knows it — no daemon involved.
        Assert.NotNull(user.Team);
        Assert.Equal(team.TeamId, user.Team.Id);
        Assert.Equal("Tiggers", user.Team.Name);
        Assert.Equal(TeamRole.Recruiter, user.Team.Role);

        await AssertOnRoster(team.TeamId, user.Id);

        await AssertUserTeam(
            user.Id,
            t =>
            {
                Assert.NotNull(t);
                Assert.Equal(team.TeamId, t.Id);
            }
        );
    }

    [Fact]
    public async Task ShouldCreateAUserWithoutATeam()
    {
        var user = await UserClient.CreateAsync(_organizationId, "loner@test.com");

        Assert.Null(user.Team);

        await AssertUserTeam(user.Id, Assert.Null);
    }

    [Fact]
    public async Task ShouldRejectAnUnknownTeam()
    {
        var response = await UserClient.CreateRawAsync(
            _organizationId,
            "nosuchteam@test.com",
            Guid.NewGuid(),
            TeamRole.Sales
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldRejectATeamWithoutARole()
    {
        var team = await TeamClient.CreateAsync(
            _organizationId,
            members: (Guid.NewGuid(), TeamRole.Sales)
        );

        var response = await UserClient.CreateRawAsync(
            _organizationId,
            "roleless@test.com",
            team.TeamId
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldRejectPuttingSomebodyOnASecondTeam()
    {
        var user = Guid.NewGuid();

        await TeamClient.CreateAsync(_organizationId, "First", (user, TeamRole.Sales));

        var second = await TeamClient.CreateAsync(
            _organizationId,
            "Second",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        var response = await TeamClient.AddMemberRawAsync(
            _organizationId,
            second.TeamId,
            user,
            TeamRole.Recruiter
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldSetTheTeamWhenAnExistingUserIsAdded()
    {
        var team = await TeamClient.CreateAsync(
            _organizationId,
            "Joiners",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        var user = await UserClient.CreateAsync(_organizationId, "joining@test.com");

        Assert.Null(user.Team);

        await TeamClient.AddMemberAsync(_organizationId, team.TeamId, user.Id, TeamRole.Recruiter);

        await AssertUserTeam(
            user.Id,
            t =>
            {
                Assert.NotNull(t);
                Assert.Equal("Joiners", t.Name);
                Assert.Equal(TeamRole.Recruiter, t.Role);
            }
        );
    }

    [Fact]
    public async Task ShouldClearTheTeamWhenTheMemberIsRemoved()
    {
        var team = await TeamClient.CreateAsync(
            _organizationId,
            "Leavers",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        var user = await UserClient.CreateAsync(
            _organizationId,
            "leaving@test.com",
            teamId: team.TeamId,
            teamRole: TeamRole.Recruiter
        );

        await AssertOnRoster(team.TeamId, user.Id);

        await TeamClient.RemoveMemberAsync(_organizationId, team.TeamId, user.Id);

        await AssertUserTeam(user.Id, Assert.Null);
    }

    [Fact]
    public async Task ShouldFollowTheTeamNameWhenTheTeamIsRenamed()
    {
        var team = await TeamClient.CreateAsync(
            _organizationId,
            "Before",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        var user = await UserClient.CreateAsync(
            _organizationId,
            "renamed@test.com",
            teamId: team.TeamId,
            teamRole: TeamRole.Recruiter
        );

        await AssertOnRoster(team.TeamId, user.Id);

        await TeamClient.RenameAsync(_organizationId, team.TeamId, "After");

        await AssertUserTeam(
            user.Id,
            t =>
            {
                Assert.NotNull(t);
                Assert.Equal("After", t.Name);
            }
        );
    }

    /// <summary>
    /// The roster is the part that travels between modules, so unlike the 201 body it always needs
    /// the daemon to have caught up.
    /// </summary>
    private async Task AssertOnRoster(Guid teamId, Guid userId)
    {
        await Eventually.AssertAsync(async () =>
        {
            var projection = await TeamClient.GetAsync(_organizationId, teamId);

            Assert.Contains(projection.Members, m => m.User.Id == userId);
        });
    }

    private async Task AssertUserTeam(Guid userId, Action<TeamInfo?> assert)
    {
        Client.WithOrganizationId(_organizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await Client.GetAsync($"/api/users/{userId}");

            response.EnsureSuccessStatusCode();

            var user = await response.ReadWithJson<UserProjection>(OutputHelper);

            Assert.NotNull(user);

            assert(user.Team);
        });
    }
}
