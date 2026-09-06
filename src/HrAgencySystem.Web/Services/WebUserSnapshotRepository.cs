using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Web.Services;

public sealed class WebUserSnapshotRepository : IUserSnapshotRepository
{
    public Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return Task.FromResult((UserSnapshot?)null);
    }
}