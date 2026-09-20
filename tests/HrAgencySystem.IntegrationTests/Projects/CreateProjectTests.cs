using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.ValueObjects;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

[Collection(IntegrationCollection.Name)]
public class CreateProjectTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task Post_valid_project_creates_project()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(organizationId);
        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, legalEntityId)
        );

        var result = await response.ReadWithJson<ProjectCreated>(OutputHelper);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.ProjectId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(companyId, result.Company.Id);
    }

    [Fact]
    public async Task Created_project_reaches_the_read_model_as_a_draft()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal(ProjectStatus.Draft, projection.Status);
            Assert.Equal(companyId, projection.CompanyId);
            Assert.Equal("BE", projection.WorkCountry);
            Assert.Equal("Bruxelles", projection.WorkplaceAddress.City);
            Assert.Null(projection.ResponsibleContact);
            Assert.Null(projection.Contract);
        });
    }

    [Fact]
    public async Task Created_project_carries_the_compliance_requirements_of_its_country_and_engagement()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);

            // Nothing recorded yet, so every requirement is outstanding.
            Assert.True(projection.ComplianceRequiredCount > 0);
            Assert.Equal(projection.ComplianceRequiredCount, projection.ComplianceOutstandingCount);
        });
    }

    [Fact]
    public async Task A_polish_project_has_nothing_to_track()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(organizationId);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, legalEntityId) with
            {
                CountryCode = "pl",
                PostalCode = "00-838",
                City = "Warszawa",
                Street = "Prosta",
            }
        );
        var project = await response.ReadWithJson<ProjectCreated>(OutputHelper);
        Assert.NotNull(project);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal(0, projection.ComplianceRequiredCount);
            Assert.Equal(0, projection.ComplianceOutstandingCount);
        });
    }

    [Fact]
    public async Task Post_project_with_invalid_fields_returns_all_validation_errors()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(organizationId);
        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, legalEntityId) with
            {
                Name = " ",
                City = " ",
                EndsOn = ProjectTestData.StartsOn.AddDays(-1),
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<BadRequestDetails>(OutputHelper);
        Assert.NotNull(problem);
        Assert.Equal(3, problem.ValidationErrors.Count);
        Assert.Contains(ProjectName.RequiredMessage, problem.ValidationErrors);
        Assert.Contains(Assignment.EndsBeforeStartMessage, problem.ValidationErrors);
    }

    [Fact]
    public async Task Post_project_for_a_company_of_another_organization_is_refused()
    {
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(owner);
        var legalEntityId = await ProjectClient.CreateLegalEntityAsync(intruder);

        Client.WithOrganizationId(intruder);
        var response = await Client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(companyId, legalEntityId)
        );

        // A business rule rather than a 404: answering "not found" would confirm whether that id
        // exists in somebody else's tenant.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.NotNull(problem);
        Assert.Equal(IProjectService.CompanyNotInOrganizationMessage, problem.Detail);
    }

    [Fact]
    public async Task Get_project_of_another_organization_returns_not_found()
    {
        var owner = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(owner);
        var project = await ProjectClient.CreateAsync(owner, companyId);

        Client.WithOrganizationId(Guid.NewGuid());

        await Eventually.AssertAsync(async () =>
        {
            var response = await Client.GetAsync($"/api/projects/{project.ProjectId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Fact]
    public async Task Listing_projects_only_shows_this_organizations_projects()
    {
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();

        var companyId = await ProjectClient.CreateCompanyAsync(owner);
        var project = await ProjectClient.CreateAsync(owner, companyId);

        var otherCompany = await ProjectClient.CreateCompanyAsync(other);
        await ProjectClient.CreateAsync(other, otherCompany);

        Client.WithOrganizationId(owner);

        await Eventually.AssertAsync(async () =>
        {
            var response = await Client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();

            var slice =
                await response.ReadWithJson<HrAgencySystem.SharedKernel.Web.SliceResponse<HrAgencySystem.Projects.Projections.ProjectProjection>>();

            Assert.NotNull(slice);
            Assert.Single(slice.Content);
            Assert.Equal(project.ProjectId, slice.Content[0].Id);
        });
    }
}
