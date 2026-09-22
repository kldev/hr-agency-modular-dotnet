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
