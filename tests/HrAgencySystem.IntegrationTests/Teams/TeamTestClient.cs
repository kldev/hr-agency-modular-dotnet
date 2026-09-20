using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Teams.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Projections;
using Xunit.Abstractions;
using TeamCreated = HrAgencySystem.Teams.Events.TeamCreated;
using TeamMemberAdded = HrAgencySystem.Teams.Events.TeamMemberAdded;
using TeamMemberRemoved = HrAgencySystem.Teams.Events.TeamMemberRemoved;
using TeamMemberRoleChanged = HrAgencySystem.Teams.Events.TeamMemberRoleChanged;
using TeamRenamed = HrAgencySystem.Teams.Events.TeamRenamed;

namespace HrAgencySystem.IntegrationTests.Teams;

public sealed class TeamTestClient(HttpClient client, ITestOutputHelper output)
{
    public const string BaseUrl = "/api/teams";

    private static readonly Random Random = new();

    internal async Task<TeamCreated> CreateAsync(
        Guid organizationId,
        string? name = null,
        params (Guid UserId, TeamRole Role)[] members
    )
    {
        var roster = members.Length > 0 ? members : [(Guid.NewGuid(), TeamRole.Sales)];

        var request = new MapCreate.CreateTeamRequest(
            name ?? "Team " + Random.Next(9999),
            [.. roster.Select(m => new MapCreate.TeamMemberRequest(m.UserId, m.Role))]
        );

        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(BaseUrl, request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<TeamCreated>();

        Assert.NotNull(result);

        return result;
    }

    internal async Task<HttpResponseMessage> CreateRawAsync(
        Guid organizationId,
        string name,
        params (Guid UserId, TeamRole Role)[] members
    )
    {
        var request = new MapCreate.CreateTeamRequest(
            name,
            [.. members.Select(m => new MapCreate.TeamMemberRequest(m.UserId, m.Role))]
        );

        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(BaseUrl, request);
    }

    internal async Task<TeamRenamed> RenameAsync(Guid organizationId, Guid teamId, string name)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"{BaseUrl}/{teamId}/name",
            new MapRename.RenameTeamRequest(name)
        );

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<TeamRenamed>(output);

        Assert.NotNull(result);

        return result;
    }

    internal async Task<HttpResponseMessage> AddMemberRawAsync(
        Guid organizationId,
        Guid teamId,
        Guid userId,
        TeamRole role
    )
    {
        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync(
            $"{BaseUrl}/{teamId}/members",
            new MapAddMember.AddTeamMemberRequest(userId, role)
        );
    }

    internal async Task<TeamMemberAdded> AddMemberAsync(
        Guid organizationId,
        Guid teamId,
        Guid userId,
        TeamRole role
    )
    {
        var response = await AddMemberRawAsync(organizationId, teamId, userId, role);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<TeamMemberAdded>(output);

        Assert.NotNull(result);

        return result;
    }

    internal async Task<HttpResponseMessage> RemoveMemberRawAsync(
        Guid organizationId,
        Guid teamId,
        Guid userId
    )
    {
        client.WithOrganizationId(organizationId);

        return await client.DeleteAsync($"{BaseUrl}/{teamId}/members/{userId}");
    }

    internal async Task<TeamMemberRemoved> RemoveMemberAsync(
        Guid organizationId,
        Guid teamId,
        Guid userId
    )
    {
        var response = await RemoveMemberRawAsync(organizationId, teamId, userId);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<TeamMemberRemoved>(output);

        Assert.NotNull(result);

        return result;
    }

    internal async Task<TeamMemberRoleChanged> ChangeMemberRoleAsync(
        Guid organizationId,
        Guid teamId,
        Guid userId,
        TeamRole role
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"{BaseUrl}/{teamId}/members/{userId}/role",
            new MapChangeMemberRole.ChangeTeamMemberRoleRequest(role)
        );

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<TeamMemberRoleChanged>(output);

        Assert.NotNull(result);

        return result;
    }

    internal async Task<SliceResponse<TeamProjection>> GetSliceAsync(
        Guid organizationId,
        string? search = null,
        Guid? userId = null
    )
    {
        client.WithOrganizationId(organizationId);

        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");

        if (userId.HasValue)
            query.Add($"userId={userId}");

        var url = query.Count > 0 ? $"{BaseUrl}?{string.Join("&", query)}" : BaseUrl;

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<SliceResponse<TeamProjection>>();

        Assert.NotNull(result);

        return result;
    }

    internal async Task<TeamProjection> GetAsync(Guid organizationId, Guid teamId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"{BaseUrl}/{teamId}");

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<TeamProjection>();

        Assert.NotNull(result);

        return result;
    }
}
