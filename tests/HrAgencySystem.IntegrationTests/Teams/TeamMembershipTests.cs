using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Teams.Application.Members.Add;
using HrAgencySystem.Teams.Application.Members.Remove;
using HrAgencySystem.Teams.Domain;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Teams;

[Collection(IntegrationCollection.Name)]
public sealed class TeamMembershipTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _sales = Guid.NewGuid();
    private readonly Guid _recruiter = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTeams();
    }

    [Fact]
    public async Task ShouldAddAMemberAndShowThemInTheReadModel()
    {
        var team = await CreateTeam();

        var added = await TeamClient.AddMemberAsync(
            _organizationId,
            team,
            _recruiter,
            TeamRole.Recruiter
        );

        Assert.Equal(_recruiter, added.Member.User.Id);
        Assert.Equal(TeamRole.Recruiter, added.Member.Role);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await TeamClient.GetAsync(_organizationId, team);

            Assert.Equal(2, projection.Members.Count);
            Assert.Contains(projection.Members, m => m.User.Id == _recruiter);
        });
    }

    [Fact]
    public async Task ShouldRejectAddingSomebodyAlreadyOnTheTeam()
    {
        var team = await CreateTeam();

        var response = await TeamClient.AddMemberRawAsync(
            _organizationId,
            team,
            _sales,
            TeamRole.Lead
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            AddTeamMemberHandler.DuplicateMemberMessage,
            await response.Content.ReadAsStringAsync()
        );
    }

    [Fact]
    public async Task ShouldRemoveAMemberWhenSomebodyElseStays()
    {
        var team = await CreateTeam();
        await TeamClient.AddMemberAsync(_organizationId, team, _recruiter, TeamRole.Recruiter);

        var removed = await TeamClient.RemoveMemberAsync(_organizationId, team, _recruiter);

        Assert.Equal(_recruiter, removed.Member.User.Id);
        // The role is read off the aggregate, the caller never sent it.
        Assert.Equal(TeamRole.Recruiter, removed.Member.Role);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await TeamClient.GetAsync(_organizationId, team);

            Assert.Single(projection.Members);
            Assert.Equal(_sales, projection.Members[0].User.Id);
        });
    }

    [Fact]
    public async Task ShouldRefuseToEmptyTheTeam()
    {
        var team = await CreateTeam();

        var response = await TeamClient.RemoveMemberRawAsync(_organizationId, team, _sales);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            RemoveTeamMemberHandler.LastMemberMessage,
            await response.Content.ReadAsStringAsync()
        );
    }

    [Fact]
    public async Task ShouldChangeAMembersRole()
    {
        var team = await CreateTeam();
        await TeamClient.AddMemberAsync(_organizationId, team, _recruiter, TeamRole.Recruiter);

        var changed = await TeamClient.ChangeMemberRoleAsync(
            _organizationId,
            team,
            _recruiter,
            TeamRole.Lead
        );

        Assert.Equal(TeamRole.Lead, changed.Member.Role);
        Assert.Equal(TeamRole.Recruiter, changed.PreviousRole);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await TeamClient.GetAsync(_organizationId, team);

            var member = Assert.Single(projection.Members, m => m.User.Id == _recruiter);
            Assert.Equal(TeamRole.Lead, member.Role);
        });
    }

    [Fact]
    public async Task ShouldRenameTheTeam()
    {
        var team = await CreateTeam();

        await TeamClient.RenameAsync(_organizationId, team, "Tiggers");

        await Eventually.AssertAsync(async () =>
        {
            var projection = await TeamClient.GetAsync(_organizationId, team);

            Assert.Equal("Tiggers", projection.Name);
        });
    }

    [Fact]
    public async Task ShouldFindTheTeamsAPersonIsOn()
    {
        var mine = await CreateTeam();
        await TeamClient.CreateAsync(
            _organizationId,
            "Somebody else's",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        await Eventually.AssertAsync(async () =>
        {
            var slice = await TeamClient.GetSliceAsync(_organizationId, userId: _sales);

            var only = Assert.Single(slice.Content);
            Assert.Equal(mine, only.Id);
        });
    }

    [Fact]
    public async Task ShouldRefuseToTouchAnotherOrganizationsTeam()
    {
        var team = await CreateTeam();

        var response = await TeamClient.AddMemberRawAsync(
            Guid.NewGuid(),
            team,
            _recruiter,
            TeamRole.Recruiter
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateTeam()
    {
        var created = await TeamClient.CreateAsync(
            _organizationId,
            "ShawSzenk",
            (_sales, TeamRole.Sales)
        );

        return created.TeamId;
    }
}
