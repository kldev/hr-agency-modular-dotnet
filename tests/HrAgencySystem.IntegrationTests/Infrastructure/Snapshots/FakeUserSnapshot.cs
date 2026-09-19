using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeUserSnapshot : IUserSnapshotRepository
{
    public Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var result = new UserSnapshot(userId, "Test", "User", "test.user@test.io");

        return Task.FromResult((UserSnapshot?)result);
    }

    // Deliberately permissive: this fake is shared by every module's integration tests, so it
    // resolves any id for any organization. The "member must belong to the organization" rule is
    // therefore covered by unit tests only.
    public Task<UserSnapshot?> GetUserAsync(
        Guid userId,
        OrganizationId organizationId,
        CancellationToken ct
    ) => GetUserAsync(userId, ct);
}
