using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Sales.Application.Queries;

public interface IOpportunityQueryRepository
{
    Task<SliceResponse<OpportunityProjection>>  GetSlicesAsync(Guid organizationId, OpportunityQuery query, CancellationToken ct);
    Task<OpportunityProjection?> GetByIdAsync(Guid organizationId, Guid opportunityId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record OpportunityQuery(
    string Search, 
    Guid? CompanyId, 
    Guid? ResponsibleId, 
    OpportunityStage? Stage, 
    int Page, 
    int PageSize):IPagedQuery;