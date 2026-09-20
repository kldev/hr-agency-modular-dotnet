using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

/// <summary>
/// Builds project fixtures through the real HTTP surface, so a test never sets up a state the API
/// could not have produced.
/// </summary>
public sealed class ProjectTestClient(HttpClient client, ITestOutputHelper output)
{
    public async Task<Guid> CreateCompanyWithProfileAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var company = await CreateCompanyAsync(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/companies/{company}/profile",
            ProjectTestData.CompanyProfile()
        );
        response.EnsureSuccessStatusCode();

        return company;
    }

    public async Task<Guid> CreateCompanyAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var request = new HrAgencySystem.Api.Endpoints.Company.Maps.MapCreate.CreateCompanyRequest(
            "Company " + Guid.NewGuid().ToString()[..8],
            "pl",
            "TX" + Guid.NewGuid().ToString()[..12],
            "REG" + Guid.NewGuid().ToString()[..8],
            "",
            HrAgencySystem.Company.Domain.Industry.Accounting
        );

        var response = await client.PostAsJsonAsync("/api/companies", request);
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<HrAgencySystem.Company.Events.CompanyCreated>(
            output
        );

        Assert.NotNull(created);

        return created.CompanyId;
    }

    public async Task<ProjectCreated> CreateAsync(
        Guid organizationId,
        Guid companyId,
        EngagementType engagementType = EngagementType.TemporaryAgencyWork,
        string countryCode = "be",
        Guid? teamId = null
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(
                companyId,
                engagementType: engagementType,
                countryCode: countryCode,
                teamId: teamId
            )
        );
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<ProjectCreated>(output);
        Assert.NotNull(created);

        return created;
    }

    /// <summary>A project with everything it takes to go live, stopping short of going live.</summary>
    public async Task<ProjectCreated> CreateReadyToGoLiveAsync(Guid organizationId)
    {
        var companyId = await CreateCompanyWithProfileAsync(organizationId);
        var project = await CreateAsync(organizationId, companyId);

        await AssignContactAsync(organizationId, project.ProjectId, ContactRole.Responsible);
        await RecordContractAsync(organizationId, project.ProjectId);

        return project;
    }

    public async Task AssignContactAsync(
        Guid organizationId,
        Guid projectId,
        ContactRole role,
        string firstName = "Marie",
        string email = "marie@acme.example.com"
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/projects/{projectId}/contacts/{role}",
            ProjectTestData.ContactRequest(firstName, email)
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task RecordContractAsync(
        Guid organizationId,
        Guid projectId,
        ContractStatus status = ContractStatus.Signed
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/projects/{projectId}/contract",
            ProjectTestData.ContractRequest(status: status)
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangeStatusAsync(Guid organizationId, Guid projectId, ProjectStatus status)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/projects/{projectId}/status",
            new MapChangeStatus.ChangeProjectStatusRequest(status, null)
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task<ProjectProjection?> GetAsync(Guid organizationId, Guid projectId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/projects/{projectId}");
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<ProjectProjection>();
    }
}
