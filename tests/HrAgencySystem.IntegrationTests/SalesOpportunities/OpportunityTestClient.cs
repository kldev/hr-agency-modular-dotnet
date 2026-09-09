using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesOpportunities;

public sealed class OpportunityTestClient(HttpClient client, ITestOutputHelper output)
{
    public const string BaseUrl = "/api/sales/opportunity";

    internal MapCreate.CreateOpportunityRequest CreateValidRequest(
        Guid? companyId = null,
        string? title = null,
        string? description = null,
        CurrencyCode? currency = null,
        DateTimeOffset? expectedCloseDate = null,
        decimal? expectedValue = null,
        Guid? responsibleId = null
    )
    => new(
        CompanyId: companyId ?? Guid.NewGuid(),
        Title: title ?? "Some title",
        Description: description ?? "",
        ExpectedValue: expectedValue ?? 10_000,
        IsHotLead: false,
        Currency: currency ?? CurrencyCode.EUR,
        ExpectedCloseDate: expectedCloseDate,
        ResponsibleId: responsibleId
    );
    
    internal MapUpdate.UpdateOpportunityRequest CreateValidUpdateRequest(
        string? title = null,
        string? description = null,
        CurrencyCode? currency = null,
        DateTimeOffset? expectedCloseDate = null,
        decimal? expectedValue = null
    )
        => new(
            Title: title ?? "Some title",
            Description: description ?? "",
            ExpectedValue: expectedValue ?? 10_000,
            false ,
            Currency: currency ?? CurrencyCode.EUR,
            ExpectedCloseDate: expectedCloseDate
        );
    
    internal async Task<OpportunityCreated> Create(
        Guid? organizationId = null,
        Guid? companyId = null,
        string? title = null,
        string? description = null,
        CurrencyCode? currency = null,
        DateTimeOffset? expectedCloseDate = null,
        decimal? expectedValue = null,
        Guid? responsibleId = null,
        Guid? createdById = null,
        bool? isHotLead = null
    )
    {
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());
        client.WithUserId(createdById ?? Guid.NewGuid());

        var request = new MapCreate.CreateOpportunityRequest(
            CompanyId: companyId ?? Guid.NewGuid(),
            Title: title ?? "Some title",
            Description: description ?? "",
            Currency: currency ?? CurrencyCode.EUR,
            ExpectedCloseDate: expectedCloseDate,
            ExpectedValue: expectedValue ?? 10_000,
            ResponsibleId: responsibleId,
            IsHotLead: isHotLead ?? false
        );

        var response = await client.PostAsJsonAsync(BaseUrl, request);

        response.EnsureSuccessStatusCode();

        var result = (await response.ReadWithJson<OpportunityCreated>())!;
        output.WriteLine($"Opportunity created: {result.OpportunityId}");

        return result;
    }
    
    internal async Task<OpportunityUpdated> Update(
        Guid? organizationId = null,
        Guid? opportunityId = null,
        string? title = null,
        string? description = null,
        CurrencyCode? currency = null,
        DateTimeOffset? expectedCloseDate = null,
        decimal? expectedValue = null,
        Guid? modifiedBy = null,
        bool? isHotLead = null
    )
    {
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());
        client.WithUserId(modifiedBy ?? Guid.NewGuid());

        var request = new MapUpdate.UpdateOpportunityRequest(
            Title: title ?? "Some title",
            Description: description ?? "",
            ExpectedValue: expectedValue ?? 10_000,
            isHotLead ?? false ,
            Currency: currency ?? CurrencyCode.EUR,
            ExpectedCloseDate: expectedCloseDate
        );

        var response = await client.PutAsJsonAsync(BaseUrl +$"/{opportunityId}",  request);

        response.EnsureSuccessStatusCode();

        var result = (await response.ReadWithJson<OpportunityUpdated>())!;
        
        return result;
    }
    
    internal async Task<OpportunityUpdated> ChangeResponsible(
        Guid? organizationId = null,
        Guid? opportunityId = null,
        Guid? responsible = null,
        Guid? modifiedBy = null
    )
    {
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());
        client.WithUserId(modifiedBy ?? Guid.NewGuid());

        var request = new ChangeResponsiblePersonRequest(
            responsible ?? Guid.NewGuid());

        var response = await client.PostAsJsonAsync(BaseUrl +$"/{opportunityId}/responsible", request);

        response.EnsureSuccessStatusCode();

        var result = (await response.ReadWithJson<OpportunityUpdated>())!;
        
        return result;
    }
}