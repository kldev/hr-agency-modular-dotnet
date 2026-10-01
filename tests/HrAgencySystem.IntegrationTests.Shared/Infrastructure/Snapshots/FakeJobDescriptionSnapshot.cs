using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public sealed class FakeJobDescriptionSnapshot : IJobDescriptionSnapshotRepository
{
    /// <summary>
    /// Which company a description is said to belong to. Null - the default - is a fresh one per
    /// call, which is all most tests need: they only care that there is some company id.
    /// <para>
    /// A test that has to reach the company it actually created pins it here and puts it back
    /// afterwards. That is not a nicety: a fresh id per call means every job post lands on its own
    /// company stream, so two posts never contend for one, and a concurrency bug on that stream
    /// cannot be reproduced at all. One was, in the seeder, which is what this exists for.
    /// </para>
    /// </summary>
    public static Guid? PinnedCompanyId { get; set; }

    public Task<JobDescriptionSnapshot?> GetAsync(
        Guid jobDescriptionId,
        Guid organizationId,
        CancellationToken ct
    )
    {
        var result = new JobDescriptionSnapshot(
            jobDescriptionId,
            "Test",
            PinnedCompanyId ?? Guid.NewGuid()
        );

        return Task.FromResult((JobDescriptionSnapshot?)result);
    }
}
