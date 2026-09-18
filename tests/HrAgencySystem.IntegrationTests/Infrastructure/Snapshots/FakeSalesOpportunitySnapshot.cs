using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Events.Opportunity;
using Marten;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public sealed class FakeSalesOpportunitySnapshot(IQuerySession session) : ISalesOpportunitySnapshotRepository
{
    public async Task<OpportunitySnapshot?> GetSnapshot(Guid opportunityId, Guid organizationId, CancellationToken ct)
    {
        // fixtures that really created an opportunity get its company, the rest of the
        // fixtures only need any snapshot for an id they made up
        var created = await session.Query<OpportunityCreated>()
            .Where(z => z.OrganizationId == organizationId)
            .Where(z => z.OpportunityId == opportunityId)
            .Select(z => new OpportunitySnapshot(z.OpportunityId, z.OrganizationId, z.Company.Id))
            .FirstOrDefaultAsync(ct);

        return created ?? new OpportunitySnapshot(opportunityId, organizationId, Guid.NewGuid());
    }
}
