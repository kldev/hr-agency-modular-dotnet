using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Web.Services;

public sealed class WebUserSnapshotRepository : IUserSnapshotRepository
{
    public Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return Task.FromResult((UserSnapshot?)null);
    }

    public Task<UserSnapshot?> GetUserAsync(
        Guid userId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        return Task.FromResult((UserSnapshot?)null);
    }
}
