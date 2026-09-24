using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.SharedKernel.Snapshots;

/// <summary>
/// A sales opportunity, as seen by a module that does not own sales: a task or a project that
/// says which deal it belongs to. Implemented by the sales module.
/// </summary>
public interface IOpportunitySnapshotRepository
{
    /// <summary>
    /// Resolves an opportunity only when it belongs to the given organization, so an id copied from
    /// another tenant answers exactly like one that does not exist.
    /// </summary>
    Task<OpportunitySnapshot?> GetOpportunityAsync(
        Guid opportunityId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

/// <summary>
/// The company is on the snapshot because every caller has the same second question - "is this
/// deal with the company I am talking about" - and answering it must not need another lookup.
/// </summary>
public sealed record OpportunitySnapshot(
    Guid OpportunityId,
    Guid OrganizationId,
    Guid CompanyId,
    string Title
);
