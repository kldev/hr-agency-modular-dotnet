using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public sealed class FakeJobDescriptionSnapshot: IJobDescriptionSnapshotRepository
{
    public Task<JobDescriptionSnapshot?> GetAsync(Guid jobDescriptionId, Guid organizationId, CancellationToken ct)
    {
        var result = new JobDescriptionSnapshot(jobDescriptionId, "Test", Guid.NewGuid());
        return Task.FromResult((JobDescriptionSnapshot?)result);
    }
}