using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Application.ChangeStatus;
using HrAgencySystem.Projects.Domain;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

[Collection(IntegrationCollection.Name)]
public class ProjectLifecycleTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task A_project_walks_from_draft_to_completed()
    {
        var organizationId = Guid.NewGuid();
        var project = await ProjectClient.CreateReadyToGoLiveAsync(organizationId);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Active
        );
        await AssertStatus(organizationId, project.ProjectId, ProjectStatus.Active);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Suspended
        );
        await AssertStatus(organizationId, project.ProjectId, ProjectStatus.Suspended);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Active
        );
        await AssertStatus(organizationId, project.ProjectId, ProjectStatus.Active);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Completed
        );
        await AssertStatus(organizationId, project.ProjectId, ProjectStatus.Completed);
    }

    [Fact]
    public async Task Going_live_without_a_signed_contract_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyWithProfileAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);
        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.Responsible
        );

        var problem = await Activate(organizationId, project.ProjectId);

        Assert.Equal(ChangeProjectStatusHandler.ContractRequiredMessage, problem.Detail);
    }

    [Fact]
    public async Task Going_live_without_a_responsible_contact_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyWithProfileAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);
        await ProjectClient.RecordContractAsync(organizationId, project.ProjectId);

        var problem = await Activate(organizationId, project.ProjectId);

        Assert.Equal(ChangeProjectStatusHandler.ResponsibleRequiredMessage, problem.Detail);
    }

    [Fact]
    public async Task Recording_a_contract_against_an_incomplete_client_profile_is_refused()
    {
        // The paperwork gate bites at the contract, before it bites at going live: a contract needs
        // a legally named party and there is nothing to name yet.
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}/contract",
            ProjectTestData.ContractRequest()
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_completed_project_does_not_restart()
    {
        var organizationId = Guid.NewGuid();
        var project = await ProjectClient.CreateReadyToGoLiveAsync(organizationId);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Active
        );
        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Completed
        );

        var problem = await Activate(organizationId, project.ProjectId);

        Assert.Equal(ChangeProjectStatusHandler.TransitionNotAllowedMessage, problem.Detail);
    }

    [Fact]
    public async Task A_draft_can_be_cancelled_outright()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Cancelled
        );

        await AssertStatus(organizationId, project.ProjectId, ProjectStatus.Cancelled);
    }

    [Fact]
    public async Task Changing_a_project_of_another_organization_is_forbidden()
    {
        var owner = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(owner);
        var project = await ProjectClient.CreateAsync(owner, companyId);

        Client.WithOrganizationId(Guid.NewGuid());

        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}/status",
            new MapChangeStatus.ChangeProjectStatusRequest(ProjectStatus.Cancelled, null)
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Updating_a_project_changes_its_details_and_period()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}",
            ProjectTestData.UpdateRequest()
        );
        response.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal("Delivery for ACME - phase two", projection.Name);
            Assert.Equal("18", projection.WorkplaceAddress.BuildingNumber);
            Assert.Equal(ProjectTestData.StartsOn.AddMonths(6), projection.EndsOn);
        });
    }

    private async Task<ProblemDetails> Activate(Guid organizationId, Guid projectId)
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{projectId}/status",
            new MapChangeStatus.ChangeProjectStatusRequest(ProjectStatus.Active, null)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.NotNull(problem);

        return problem;
    }

    private async Task AssertStatus(Guid organizationId, Guid projectId, ProjectStatus expected) =>
        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, projectId);

            Assert.NotNull(projection);
            Assert.Equal(expected, projection.Status);
        });
}
