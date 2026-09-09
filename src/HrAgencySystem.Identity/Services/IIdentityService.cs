using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Identity.Services;

public interface IIdentityService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
}