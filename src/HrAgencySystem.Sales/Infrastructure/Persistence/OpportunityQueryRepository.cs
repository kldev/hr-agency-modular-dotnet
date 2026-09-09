using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public sealed class OpportunityQueryRepository(IQuerySession session) : IOpportunityQueryRepository
{
    public async Task<SliceResponse<OpportunityProjection>> GetSlicesAsync(Guid organizationId, OpportunityQuery query, CancellationToken ct)
    {
        return await session.Query<OpportunityProjection>()
            .WithOrganizationId(organizationId)
            .WithOptionalCompanyId(query.CompanyId)
            .WithResponsibleId(query.ResponsibleId)
            .WithStage(query.Stage)
            .WithSearch(query.Search)
            .OrderByDescending(z=>z.CreatedAt)
            .ToSlice(query, ct);

    }

    public async Task<OpportunityProjection?> GetByIdAsync(Guid organizationId, Guid opportunityId, CancellationToken ct)
    {
        return await session.Query<OpportunityProjection>()
            .WithOrganizationId(organizationId)
            .WithOpportunityId(opportunityId)
            .SingleOrDefaultAsync(ct);
    }
}