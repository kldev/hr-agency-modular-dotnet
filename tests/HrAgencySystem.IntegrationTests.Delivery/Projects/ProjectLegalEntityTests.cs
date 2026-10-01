using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Application.LegalEntity.Change;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

[Collection(IntegrationCollection.Name)]
public class ProjectLegalEntityTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
        await Cleaner.CleanLegalEntities();
    }

    [Fact]
    public async Task A_project_records_which_of_our_companies_delivers_it()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(organizationId);

        var created = await ProjectClient.CreateAsync(
            organizationId,
            companyId,
            legalEntityId: legalEntityId
        );

        Assert.Equal(legalEntityId, created.DeliveringEntity.LegalEntityId);
        Assert.Equal("HR Agency sp. z o.o.", created.DeliveringEntity.LegalName);

        await Eventually.AssertAsync(async () =>
        {
            var project = await ProjectClient.GetAsync(organizationId, created.ProjectId);

            Assert.NotNull(project);
            Assert.Equal(legalEntityId, project.DeliveringEntity.LegalEntityId);
        });
    }

    [Fact]
    public async Task A_draft_can_still_be_moved_to_another_company()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);

        var created = await ProjectClient.CreateAsync(organizationId, companyId);
        var other = await ProjectClient.CreateLegalEntityAsync(organizationId);

        var changed = await ChangeLegalEntity(organizationId, created.ProjectId, other);

        Assert.Equal(other, changed.DeliveringEntity.LegalEntityId);

        await Eventually.AssertAsync(async () =>
        {
            var project = await ProjectClient.GetAsync(organizationId, created.ProjectId);

            Assert.NotNull(project);
            Assert.Equal(other, project.DeliveringEntity.LegalEntityId);
        });
    }

    /// <summary>
    /// The rule the whole snapshot exists for. Once the project is live its contract, its
    /// notifications and its declarations all name this company, and none of them would be put
    /// right by swapping a field - carrying on elsewhere is a new project.
    /// </summary>
    [Fact]
    public async Task Once_the_project_has_started_the_company_is_settled()
    {
        var organizationId = Guid.NewGuid();
        var created = await ProjectClient.CreateReadyToGoLiveAsync(organizationId);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            created.ProjectId,
            ProjectStatus.Active
        );

        var other = await ProjectClient.CreateLegalEntityAsync(organizationId);

        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{created.ProjectId}/legal-entity",
            new ChangeProjectLegalEntityRequest(other)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);

        Assert.NotNull(problem);
        Assert.Equal(ChangeProjectLegalEntityHandler.ProjectAlreadyStartedMessage, problem.Detail);
    }

    [Fact]
    public async Task Moving_a_draft_to_the_company_it_already_has_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(organizationId);

        var created = await ProjectClient.CreateAsync(
            organizationId,
            companyId,
            legalEntityId: legalEntityId
        );

        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{created.ProjectId}/legal-entity",
            new ChangeProjectLegalEntityRequest(legalEntityId)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// A business rule rather than a 404, for the same reason the client side is: answering "not
    /// found" would confirm whether that id exists in somebody else's tenant.
    /// </summary>
    [Fact]
    public async Task A_company_of_another_organization_cannot_deliver_our_project()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var theirs = await ProjectClient.CreateLegalEntityAsync(Guid.NewGuid());

        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, theirs)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);

        Assert.NotNull(problem);
        Assert.Equal(IProjectService.LegalEntityNotInOrganizationMessage, problem.Detail);
    }

    /// <summary>
    /// Checked against the day the project starts, not today: setting up next month's engagement on
    /// a company that closes this month is exactly the mistake worth catching.
    /// </summary>
    [Fact]
    public async Task A_company_that_had_stopped_trading_cannot_take_the_project()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);

        Client.WithOrganizationId(organizationId);

        var closed = await Client.PostAsJsonAsync(
            "/api/legal-entities",
            LegalEntities.LegalEntityTestData.Request(
                activeFrom: new DateOnly(2019, 1, 1),
                activeTo: new DateOnly(2020, 1, 1)
            )
        );
        closed.EnsureSuccessStatusCode();

        var entity =
            await closed.ReadWithJson<HrAgencySystem.LegalEntities.Events.LegalEntityCreated>(
                OutputHelper
            );

        Assert.NotNull(entity);

        var response = await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, entity.LegalEntityId)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);

        Assert.NotNull(problem);
        Assert.Equal(IProjectService.LegalEntityNotTradingMessage, problem.Detail);
    }

    private async Task<ProjectLegalEntityChanged> ChangeLegalEntity(
        Guid organizationId,
        Guid projectId,
        Guid legalEntityId
    )
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{projectId}/legal-entity",
            new ChangeProjectLegalEntityRequest(legalEntityId)
        );

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<ProjectLegalEntityChanged>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }
}
