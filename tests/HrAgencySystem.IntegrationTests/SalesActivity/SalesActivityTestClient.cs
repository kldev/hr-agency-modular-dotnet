using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Sales.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Web;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesActivity;

public sealed class SalesActivityTestClient(HttpClient client, ITestOutputHelper output)
{
    public const string BaseUrl = "/api/sales";

    internal async Task<SalesActivityCreated> Create(
        Guid? organizationId = null,
        Guid? opportunityId = null,
        SalesActivityType? type = null,
        string? note = null,
        Guid? createdById = null)
    {
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());
        client.WithUserId(createdById ?? Guid.NewGuid());

        var request = new CreateSalesActivityRequest(
            opportunityId ?? Guid.NewGuid(), 
            type ?? RandomType(),
            note ?? "");
        
        output.WriteLine($"Create activity {request.Type}");

        var response = await client.PostAsJsonAsync(BaseUrl + "/activity", request);
        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<SalesActivityCreated>();
        output.WriteLine($"Created activity {result}");

        return result!;
    }

    private static SalesActivityType RandomType()
    {
        return Enum.GetValues<SalesActivityType>()[Random.Shared.Next(Enum.GetValues<SalesActivityType>().Length)];
    }
    
    internal async Task<SliceResponse<SalesActivityProjection>> GetSliceAsync ( 
        Guid organizationId,
        Guid? opportunityId = null,
        Guid? companyId = null,
        int page = 1,
        int pageSize = 100)
    {
        var sliceUrl = $"{BaseUrl}/activities";
        var query = new List<string>();
        
        if (companyId != null)
            query.Add($"companyId={companyId}");
        
        if (opportunityId != null)
            query.Add($"opportunityId={opportunityId}");
        
        query.Add($"page={page}");
        query.Add($"pageSize={pageSize}");
        
        client.WithOrganizationId(organizationId);
        sliceUrl += $"?{string.Join("&", query)}";

        output.WriteLine("URL :" + sliceUrl);
        var response = await client.GetAsync(
            $"{sliceUrl}");
        
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<SliceResponse<SalesActivityProjection>>())!;
    }
}