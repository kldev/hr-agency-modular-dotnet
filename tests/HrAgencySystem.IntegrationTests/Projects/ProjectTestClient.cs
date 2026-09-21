using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.Compliance;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Web;
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

    /// <summary>
    /// One of the agency's own companies, which every project now needs. Created on the spot unless
    /// a test cares which one it is.
    /// </summary>
    public async Task<Guid> CreateLegalEntityAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            "/api/legal-entities",
            LegalEntities.LegalEntityTestData.Request()
        );
        response.EnsureSuccessStatusCode();

        var created =
            await response.ReadWithJson<HrAgencySystem.LegalEntities.Events.LegalEntityCreated>(
                output
            );

        Assert.NotNull(created);

        return created.LegalEntityId;
    }

    public async Task<ProjectCreated> CreateAsync(
        Guid organizationId,
        Guid companyId,
        EngagementType engagementType = EngagementType.TemporaryAgencyWork,
        string countryCode = "be",
        Guid? teamId = null,
        Guid? legalEntityId = null
    )
    {
        var entity = legalEntityId ?? await CreateLegalEntityAsync(organizationId);

        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            "/api/projects",
            ProjectTestData.CreateRequest(
                companyId,
                entity,
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

    public async Task<ProjectPositionOpened> OpenPositionAsync(
        Guid organizationId,
        Guid projectId,
        string name = "Painter",
        string? contractName = null,
        int? plannedHeadcount = null
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/positions",
            ProjectTestData.PositionRequest(name, contractName, plannedHeadcount)
        );
        response.EnsureSuccessStatusCode();

        var opened = await response.ReadWithJson<ProjectPositionOpened>(output);
        Assert.NotNull(opened);

        return opened;
    }

    public async Task<HttpResponseMessage> OpenPositionResponseAsync(
        Guid organizationId,
        Guid projectId,
        string name
    )
    {
        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/positions",
            ProjectTestData.PositionRequest(name)
        );
    }

    public async Task ArchivePositionAsync(Guid organizationId, Guid projectId, Guid positionId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsync(
            $"/api/projects/{projectId}/positions/{positionId}/archive",
            null
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task<SliceResponse<PositionListItem>?> GetPositionsAsync(
        Guid organizationId,
        Guid? projectId = null,
        bool includeArchived = false
    )
    {
        client.WithOrganizationId(organizationId);

        var query =
            $"?includeArchived={includeArchived}"
            + (projectId is null ? "" : $"&projectId={projectId}");

        var response = await client.GetAsync($"/api/positions{query}");
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<SliceResponse<PositionListItem>>();
    }

    public async Task<ProjectProjection?> GetAsync(Guid organizationId, Guid projectId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/projects/{projectId}");
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<ProjectProjection>();
    }
}
