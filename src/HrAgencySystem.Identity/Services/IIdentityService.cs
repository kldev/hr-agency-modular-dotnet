using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Services;

public interface IIdentityService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    Task<OrganizationInfo> GetOrganization(OrganizationId organizationId, CancellationToken ct);

    /// <summary>
    /// Resolves a team in this organization, refusing with a business rule rather than a 404 so the
    /// answer never reveals whether the id exists in somebody else's tenant.
    /// </summary>
    Task<TeamSnapshot> GetTeamAsync(
        Guid teamId,
        OrganizationId organizationId,
        CancellationToken ct
    );
    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
