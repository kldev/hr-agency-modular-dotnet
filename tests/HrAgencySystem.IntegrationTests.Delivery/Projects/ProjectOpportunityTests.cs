using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

/// <summary>
/// A delivery says which sale it came from - optionally, and only a sale to the same client in
/// the same organization.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class ProjectOpportunityTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    private readonly Guid _organizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanSales();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task A_project_created_from_a_deal_names_it()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var deal = await OpportunityTestClient.Create(_organizationId, companyId, title: "Warehouse Workers");

        var response = await PostAsync(companyId, deal.OpportunityId);
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<ProjectCreated>(OutputHelper);
        Assert.Equal(deal.OpportunityId, created!.Opportunity?.Id);

        await Eventually.AssertAsync(async () =>
        {
            var project = await ProjectClient.GetAsync(_organizationId, created.ProjectId);

            Assert.Equal(deal.OpportunityId, project?.Opportunity?.Id);
            Assert.Equal("Warehouse Workers", project?.Opportunity?.Title);
        });
    }

    [Fact]
    public async Task A_deal_sold_to_another_company_is_refused()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var otherCompany = await ProjectClient.CreateCompanyAsync(_organizationId);
        var deal = await OpportunityTestClient.Create(_organizationId, otherCompany);

        var response = await PostAsync(companyId, deal.OpportunityId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(IProjectService.OpportunityOfAnotherCompanyMessage, problem!.Detail);
    }

    [Fact]
    public async Task Another_organizations_deal_is_refused()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var otherOrganization = Guid.NewGuid();
        var foreignDeal = await OpportunityTestClient.Create(
            otherOrganization,
            await ProjectClient.CreateCompanyAsync(otherOrganization)
        );

        var response = await PostAsync(companyId, foreignDeal.OpportunityId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(IProjectService.OpportunityNotInOrganizationMessage, problem!.Detail);
    }

    [Fact]
    public async Task Updating_a_project_can_link_and_unlink_a_deal()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var project = await ProjectClient.CreateAsync(_organizationId, companyId);
        var deal = await OpportunityTestClient.Create(_organizationId, companyId, title: "Maintenance Team");

        Client.WithOrganizationId(_organizationId);
        var linked = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}",
            ProjectTestData.UpdateRequest() with { SalesOpportunityId = deal.OpportunityId }
        );
        linked.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
            Assert.Equal(
                "Maintenance Team",
                (await ProjectClient.GetAsync(_organizationId, project.ProjectId))?.Opportunity?.Title
            )
        );

        Client.WithOrganizationId(_organizationId);
        var unlinked = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}",
            ProjectTestData.UpdateRequest()
        );
        unlinked.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
            Assert.Null((await ProjectClient.GetAsync(_organizationId, project.ProjectId))?.Opportunity)
        );
    }

    private async Task<HttpResponseMessage> PostAsync(Guid companyId, Guid opportunityId)
    {
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(_organizationId);
        Client.WithOrganizationId(_organizationId);

        return await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, legalEntityId) with
            {
                SalesOpportunityId = opportunityId,
            }
        );
    }
}
