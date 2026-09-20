using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Teams.Application.Create;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Teams;

[Collection(IntegrationCollection.Name)]
public sealed class CreateTeamTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTeams();
    }

    [Fact]
    public async Task ShouldCreateTeamWithTheWholeRoster()
    {
        var sales = Guid.NewGuid();
        var recruiter = Guid.NewGuid();
        var operations = Guid.NewGuid();

        var result = await TeamClient.CreateAsync(
            _organizationId,
            "ShawSzenk",
            (sales, TeamRole.Sales),
            (recruiter, TeamRole.Recruiter),
            (operations, TeamRole.Operations)
        );

        Assert.Equal(_organizationId, result.OrganizationId);
        Assert.Equal("ShawSzenk", result.Name);
        Assert.Equal(3, result.Members.Count);

        await Eventually.AssertAsync(async () =>
        {
            var team = await TeamClient.GetAsync(_organizationId, result.TeamId);

            Assert.Equal("ShawSzenk", team.Name);
            Assert.Equal(3, team.Members.Count);
            Assert.Equal([sales, recruiter, operations], team.Members.Select(m => m.User.Id));
            Assert.Equal(
                [TeamRole.Sales, TeamRole.Recruiter, TeamRole.Operations],
                team.Members.Select(m => m.Role)
            );
        });
    }

    [Fact]
    public async Task ShouldRejectATeamWithNoMembers()
    {
        var response = await TeamClient.CreateRawAsync(_organizationId, "Empty");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadAsStringAsync();
        Assert.Contains(CreateTeamHandler.EmptyMembersMessage, problem);
    }

    [Fact]
    public async Task ShouldRejectTheSamePersonTwice()
    {
        var duplicate = Guid.NewGuid();

        var response = await TeamClient.CreateRawAsync(
            _organizationId,
            "Doubled",
            (duplicate, TeamRole.Sales),
            (duplicate, TeamRole.Lead)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadAsStringAsync();
        Assert.Contains(CreateTeamHandler.DuplicateMemberMessage, problem);
    }

    [Fact]
    public async Task ShouldRejectABlankName()
    {
        var response = await TeamClient.CreateRawAsync(
            _organizationId,
            "   ",
            (Guid.NewGuid(), TeamRole.Sales)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldOnlyListTeamsOfTheCallersOrganization()
    {
        await TeamClient.CreateAsync(_organizationId, "Ours", (Guid.NewGuid(), TeamRole.Sales));
        await TeamClient.CreateAsync(Guid.NewGuid(), "Theirs", (Guid.NewGuid(), TeamRole.Sales));

        await Eventually.AssertAsync(async () =>
        {
            var slice = await TeamClient.GetSliceAsync(_organizationId);

            Assert.Single(slice.Content);
            Assert.Equal("Ours", slice.Content[0].Name);
        });
    }
}
