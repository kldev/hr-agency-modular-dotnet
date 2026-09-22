using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Application.Port;

public interface IAgencyEmploymentQueryRepository
{
    Task<AgencyEmploymentProjection?> GetAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    );

    Task<IReadOnlyList<AgencyEmploymentProjection>> GetAllAsync(
        OrganizationId organizationId,
        CancellationToken ct
    );

    /// <summary>
    /// The records of the given people, in one query - the settlement file needs a rate for every
    /// person on it. Somebody with no record is simply absent from the answer.
    /// </summary>
    Task<IReadOnlyList<AgencyEmploymentProjection>> GetForUsersAsync(
        OrganizationId organizationId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct
    );

    /// <summary>
    /// Everybody whose contract carries the duty to record hours during the given month. The filter
    /// is on the contract type and on the engagement covering the month at all - somebody who left
    /// in March still owes March.
    /// </summary>
    Task<IReadOnlyList<AgencyEmploymentProjection>> GetCoveredAsync(
        OrganizationId organizationId,
        int year,
        int month,
        CancellationToken ct
    );
}
