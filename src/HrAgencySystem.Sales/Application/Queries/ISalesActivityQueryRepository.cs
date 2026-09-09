using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Sales.Application.Queries;

public interface ISalesActivityQueryRepository
{
    Task<SliceResponse<SalesActivityProjection>> GetSlicesAsync(Guid organizationId, SalesActivityQuery query , CancellationToken ct);
}

public record SalesActivityQuery(Guid? OpportunityId, Guid? CompanyId, int Page, int PageSize) : IPagedQuery;