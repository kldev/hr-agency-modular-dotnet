using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public sealed class SalesActivityQueryRepository(IQuerySession session) : ISalesActivityQueryRepository
{
    public async Task<SliceResponse<SalesActivityProjection>> GetSlicesAsync(Guid organizationId, 
        SalesActivityQuery query, CancellationToken ct)
    {
        return await session.Query<SalesActivityProjection>()
            .WithOrganizationId(organizationId)
            .WithCompanyId(query.CompanyId)
            .WithOpportunityId(query.OpportunityId)
            .OrderByDescending(z => z.CreatedAt)
            .ToSlice(query, ct);
    }
}