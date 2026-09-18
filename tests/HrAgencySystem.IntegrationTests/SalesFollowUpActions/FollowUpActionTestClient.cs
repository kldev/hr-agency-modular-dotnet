using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.SalesFollowUpAction.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Documents;
using HrAgencySystem.Sales.Events.FollowUp;
using HrAgencySystem.SharedKernel.Web;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesFollowUpActions;

public sealed class FollowUpActionTestClient(HttpClient client, ITestOutputHelper output)
{
    public const string BaseUrl = "/api/sales/follow-up";

    internal async Task<HttpResponseMessage> CreateResponse(
        Guid? organizationId = null,
        Guid? opportunityId = null,
        string? content = null,
        DateTimeOffset? followDateTime = null,
        Guid? createdById = null)
    {
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());
        client.WithUserId(createdById ?? Guid.NewGuid());

        var request = new MapCreate.CreateFollowUpActionRequest(
            OpportunityId: opportunityId ?? Guid.NewGuid(),
            Content: content ?? "Call the client back",
            FollowDateTime: followDateTime ?? DateTimeOffset.UtcNow.AddDays(1));

        output.WriteLine($"Create follow up action {request.OpportunityId} {request.FollowDateTime:O}");

        return await client.PostAsJsonAsync(BaseUrl, request);
    }

    internal async Task<FollowUpActionCreated> Create(
        Guid? organizationId = null,
        Guid? opportunityId = null,
        string? content = null,
        DateTimeOffset? followDateTime = null,
        Guid? createdById = null)
    {
        var response = await CreateResponse(organizationId, opportunityId, content, followDateTime, createdById);
        response.EnsureSuccessStatusCode();

        var result = (await response.ReadWithJson<FollowUpActionCreated>())!;
        output.WriteLine($"Created follow up action {result.FollowUpActionId}");

        return result;
    }

    internal async Task<HttpResponseMessage> UpdateResponse(
        Guid? organizationId = null,
        Guid? followUpActionId = null,
        string? content = null,
        DateTimeOffset? followDateTime = null,
        Guid? modifiedById = null)
    {
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());
        client.WithUserId(modifiedById ?? Guid.NewGuid());

        var request = new MapUpdate.UpdateFollowUpActionRequest(
            Content: content ?? "Call the client back",
            FollowDateTime: followDateTime ?? DateTimeOffset.UtcNow.AddDays(2));

        return await client.PutAsJsonAsync($"{BaseUrl}/{followUpActionId ?? Guid.NewGuid()}", request);
    }

    internal async Task<FollowUpActionUpdated> Update(
        Guid? organizationId = null,
        Guid? followUpActionId = null,
        string? content = null,
        DateTimeOffset? followDateTime = null,
        Guid? modifiedById = null)
    {
        var response = await UpdateResponse(organizationId, followUpActionId, content, followDateTime, modifiedById);
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<FollowUpActionUpdated>())!;
    }

    internal async Task<HttpResponseMessage> GetResponse(Guid organizationId, Guid followUpActionId)
    {
        client.WithOrganizationId(organizationId);

        return await client.GetAsync($"{BaseUrl}/{followUpActionId}");
    }

    internal async Task<FollowUpAction> Get(Guid organizationId, Guid followUpActionId)
    {
        var response = await GetResponse(organizationId, followUpActionId);
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<FollowUpAction>())!;
    }

    internal async Task<SliceResponse<FollowUpAction>> GetSliceAsync(
        Guid organizationId,
        Guid? opportunityId = null,
        Guid? companyId = null,
        int page = 1,
        int pageSize = 100)
    {
        var query = new List<string>();

        if (opportunityId != null)
            query.Add($"opportunityId={opportunityId}");

        if (companyId != null)
            query.Add($"companyId={companyId}");

        query.Add($"page={page}");
        query.Add($"pageSize={pageSize}");

        client.WithOrganizationId(organizationId);

        var sliceUrl = $"{BaseUrl}?{string.Join("&", query)}";
        output.WriteLine("URL :" + sliceUrl);

        var response = await client.GetAsync(sliceUrl);
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<SliceResponse<FollowUpAction>>())!;
    }
}
