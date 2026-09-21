using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

/// <summary>
/// Roles inside a project: one client, one project, and painters and bricklayers in it. The list is
/// read across projects, so these cover both the writing side and what the register then shows.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class ProjectPositionTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task A_project_carries_several_roles()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, company);

        await ProjectClient.OpenPositionAsync(organizationId, project.ProjectId, "Painter");
        await ProjectClient.OpenPositionAsync(organizationId, project.ProjectId, "Bricklayer");

        await Eventually.AssertAsync(async () =>
        {
            var positions = await ProjectClient.GetPositionsAsync(
                organizationId,
                project.ProjectId
            );

            Assert.NotNull(positions);
            Assert.Equal(2, positions.Content.Count);
            Assert.Contains(positions.Content, p => p.Name == "Painter");
            Assert.Contains(positions.Content, p => p.Name == "Bricklayer");
        });
    }

    /// <summary>
    /// The list is what the register page reads, and it has to name the project without the
    /// position carrying a copy of that name around.
    /// </summary>
    [Fact]
    public async Task The_register_names_the_project_and_the_client()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, company);

        await ProjectClient.OpenPositionAsync(organizationId, project.ProjectId, "Painter");

        await Eventually.AssertAsync(async () =>
        {
            var positions = await ProjectClient.GetPositionsAsync(organizationId);

            Assert.NotNull(positions);
            var row = Assert.Single(positions.Content);

            Assert.Equal(project.ProjectId, row.ProjectId);
            Assert.False(string.IsNullOrWhiteSpace(row.ProjectName));
            Assert.False(string.IsNullOrWhiteSpace(row.ClientCompanyName));
        });
    }

    [Fact]
    public async Task Two_roles_cannot_share_a_name_within_one_project()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, company);

        await ProjectClient.OpenPositionAsync(organizationId, project.ProjectId, "Painter");

        var response = await ProjectClient.OpenPositionResponseAsync(
            organizationId,
            project.ProjectId,
            "painter"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>Two projects, the same role name - which is normal and has to stay allowed.</summary>
    [Fact]
    public async Task The_same_name_in_another_project_is_fine()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var first = await ProjectClient.CreateAsync(organizationId, company);
        var second = await ProjectClient.CreateAsync(organizationId, company);

        await ProjectClient.OpenPositionAsync(organizationId, first.ProjectId, "Painter");

        var response = await ProjectClient.OpenPositionResponseAsync(
            organizationId,
            second.ProjectId,
            "Painter"
        );

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task An_archived_role_leaves_the_list_but_not_the_history()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, company);

        var opened = await ProjectClient.OpenPositionAsync(
            organizationId,
            project.ProjectId,
            "Painter"
        );

        await ProjectClient.ArchivePositionAsync(
            organizationId,
            project.ProjectId,
            opened.Position.PositionId
        );

        await Eventually.AssertAsync(async () =>
        {
            var live = await ProjectClient.GetPositionsAsync(organizationId, project.ProjectId);
            Assert.NotNull(live);
            Assert.Empty(live.Content);

            var all = await ProjectClient.GetPositionsAsync(
                organizationId,
                project.ProjectId,
                includeArchived: true
            );
            Assert.NotNull(all);
            Assert.Single(all.Content);
            Assert.True(all.Content[0].IsArchived);
        });
    }

    /// <summary>
    /// "Four painters" and one of them hired: the register has to be able to say what is missing,
    /// which is the question the whole list exists for.
    /// </summary>
    [Fact]
    public async Task A_role_says_how_many_people_it_still_needs()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, company);

        await ProjectClient.OpenPositionAsync(
            organizationId,
            project.ProjectId,
            "Painter",
            plannedHeadcount: 4
        );

        await Eventually.AssertAsync(async () =>
        {
            var positions = await ProjectClient.GetPositionsAsync(
                organizationId,
                project.ProjectId
            );

            Assert.NotNull(positions);
            var row = Assert.Single(positions.Content);

            Assert.Equal(4, row.PlannedHeadcount);
            Assert.Equal(0, row.AssignedCount);
            Assert.Equal(4, row.MissingHeadcount);
        });
    }

    [Fact]
    public async Task A_role_of_another_organization_is_not_found()
    {
        var organizationId = Guid.NewGuid();
        var company = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, company);

        var opened = await ProjectClient.OpenPositionAsync(
            organizationId,
            project.ProjectId,
            "Painter"
        );

        Client.WithOrganizationId(Guid.NewGuid());

        var response = await Client.GetAsync($"/api/positions/{opened.Position.PositionId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
