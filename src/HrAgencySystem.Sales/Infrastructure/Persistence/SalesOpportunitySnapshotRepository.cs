using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Projections;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public class SalesOpportunitySnapshotRepository(IQuerySession session) : ISalesOpportunitySnapshotRepository
{
    public async Task<OpportunitySnapshot?> GetSnapshot(Guid opportunityId, Guid organizationId, CancellationToken ct)
    {
        var result = await session.Query<OpportunityProjection>()
            .WithOrganizationId(organizationId)
            .WithOpportunityId(opportunityId)
            .Select(z => new OpportunitySnapshot(z.Id, z.OrganizationId, z.CompanyId))
            .FirstOrDefaultAsync(ct);
        if (result != null) return result;

        var fromEvent = await session.Query<OpportunityCreated>()
            .Where(z => z.OrganizationId == organizationId)
            .Where(z => z.OpportunityId == organizationId)
            .Select(z => new OpportunitySnapshot(z.OpportunityId,
                z.OrganizationId, z.Company.Id)).FirstOrDefaultAsync(ct);

        return fromEvent;
    }
}