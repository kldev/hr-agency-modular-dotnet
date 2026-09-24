using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public class SalesOpportunitySnapshotRepository(IQuerySession session)
    : IOpportunitySnapshotRepository
{
    public async Task<OpportunitySnapshot?> GetOpportunityAsync(
        Guid opportunityId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<OpportunityProjection>()
            .WithOrganizationId(organizationId.Value)
            .WithOpportunityId(opportunityId)
            .Select(z => new OpportunitySnapshot(z.Id, z.OrganizationId, z.CompanyId, z.Title))
            .FirstOrDefaultAsync(ct);
        if (result != null)
            return result;

        // The projection lags the write that created the opportunity; the creation event does not.
        var fromEvent = await session
            .Query<OpportunityCreated>()
            .Where(z => z.OrganizationId == organizationId.Value)
            .Where(z => z.OpportunityId == opportunityId)
            .Select(z => new OpportunitySnapshot(
                z.OpportunityId,
                z.OrganizationId,
                z.Company.Id,
                z.Title
            ))
            .FirstOrDefaultAsync(ct);

        return fromEvent;
    }
}
