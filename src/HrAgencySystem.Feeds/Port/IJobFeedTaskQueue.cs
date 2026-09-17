using HrAgencySystem.Feeds.Model;

namespace HrAgencySystem.Feeds.Port;

internal interface IJobFeedTaskQueue
{
    Task<IReadOnlyList<JobFeedTask>> Fetch(
        int batchSize,
        CancellationToken ct);
}