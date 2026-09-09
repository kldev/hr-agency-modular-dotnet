using HrAgencySystem.Sales.Application.Queries;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public sealed class FakeSalesOpportunitySnapshot : ISalesOpportunitySnapshotRepository
{
    public Task<OpportunitySnapshot?> GetSnapshot(Guid opportunityId, Guid organizationId, CancellationToken ct)
    {
        var result = new OpportunitySnapshot(opportunityId, organizationId, Guid.NewGuid());
        return Task.FromResult((OpportunitySnapshot?)result);
    }
}