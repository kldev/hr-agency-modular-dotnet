namespace HrAgencySystem.Sales.Application.Queries;

public interface ISalesOpportunitySnapshotRepository
{
    Task<OpportunitySnapshot?> GetSnapshot(Guid opportunityId, Guid organizationId, CancellationToken ct);
}

public record OpportunitySnapshot(
    Guid OpportunityId, 
    Guid OrganizationId, 
    Guid CompanyId);