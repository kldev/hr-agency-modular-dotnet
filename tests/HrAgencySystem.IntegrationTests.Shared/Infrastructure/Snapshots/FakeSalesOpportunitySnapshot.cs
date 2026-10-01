using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public sealed class FakeSalesOpportunitySnapshot(IQuerySession session)
    : IOpportunitySnapshotRepository
{
    public async Task<OpportunitySnapshot?> GetOpportunityAsync(
        Guid opportunityId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var created = await session
            .Query<OpportunityCreated>()
            .Where(z => z.OpportunityId == opportunityId)
            .Select(z => new OpportunitySnapshot(
                z.OpportunityId,
                z.OrganizationId,
                z.Company.Id,
                z.Title
            ))
            .FirstOrDefaultAsync(ct);

        // Fixtures that really created an opportunity get it - but only in its own organization,
        // so the tenant wall is testable. The rest only need any snapshot for an id they made up.
        if (created is not null)
            return created.OrganizationId == organizationId.Value ? created : null;

        return new OpportunitySnapshot(
            opportunityId,
            organizationId.Value,
            Guid.NewGuid(),
            "Opportunity " + opportunityId.ToString()[..8]
        );
    }
}
