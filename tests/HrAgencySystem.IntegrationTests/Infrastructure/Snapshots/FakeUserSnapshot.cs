using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeUserSnapshot : IUserSnapshotRepository
{
    public Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var result = new UserSnapshot(userId, "Test", "User", "test.user@test.io");
        
        return Task.FromResult((UserSnapshot?)result);
    }
}