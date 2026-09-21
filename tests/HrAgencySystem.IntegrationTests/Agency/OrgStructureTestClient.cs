using System.Net.Http.Json;
using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Endpoints.OrgStructure.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Agency;

/// <summary>Builds a chart through the real HTTP surface, so no test sets up a shape the API could not.</summary>
public sealed class OrgStructureTestClient(HttpClient client, ITestOutputHelper output)
{
    private const string Base = "/api/org-structure";

    public async Task<Guid> CreateUnitAsync(
        Guid organizationId,
        Guid? parentId,
        string name,
        OrgUnitKind kind = OrgUnitKind.Department
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            $"{Base}/units",
            new MapCreateUnit.CreateOrgUnitRequest(parentId, name, kind)
        );
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<OrgUnitCreated>(output);
        Assert.NotNull(created);

        return created.UnitId;
    }

    public async Task<HttpResponseMessage> CreateUnitResponseAsync(
        Guid organizationId,
        Guid? parentId,
        string name
    )
    {
        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(
            $"{Base}/units",
            new MapCreateUnit.CreateOrgUnitRequest(parentId, name, OrgUnitKind.Department)
        );
    }

    public async Task AddMemberAsync(
        Guid organizationId,
        Guid unitId,
        Guid userId,
        string? title = null
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            $"{Base}/units/{unitId}/members",
            new MapAddMember.AddOrgUnitMemberRequest(userId, title)
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignHeadAsync(Guid organizationId, Guid unitId, Guid headUserId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"{Base}/units/{unitId}/head",
            new MapAssignHead.AssignOrgUnitHeadRequest(headUserId)
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task<HttpResponseMessage> MoveUnitResponseAsync(
        Guid organizationId,
        Guid unitId,
        Guid parentId
    )
    {
        client.WithOrganizationId(organizationId);

        return await client.PutAsJsonAsync(
            $"{Base}/units/{unitId}/parent",
            new MapMoveUnit.MoveOrgUnitRequest(parentId)
        );
    }

    public async Task<SupervisorView?> GetSupervisorAsync(Guid organizationId, Guid userId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"{Base}/supervisor/{userId}");
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<SupervisorView>();
    }

    public async Task<IReadOnlyList<Guid>> GetSubordinatesAsync(
        Guid organizationId,
        Guid userId,
        bool wholeSubtree
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync(
            $"{Base}/subordinates/{userId}?wholeSubtree={wholeSubtree}"
        );
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<List<Guid>>() ?? [];
    }

    public async Task<OrgStructureProjection?> GetAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync(Base);
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<OrgStructureProjection>();
    }
}
