using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Services;

public interface IIdentityService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    Task<OrganizationInfo> GetOrganization(OrganizationId organizationId, CancellationToken ct);
}