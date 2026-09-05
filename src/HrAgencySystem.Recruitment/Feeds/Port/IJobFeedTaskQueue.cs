using HrAgencySystem.Recruitment.Feeds.Model;

namespace HrAgencySystem.Recruitment.Feeds.Port;

internal interface IJobFeedTaskQueue
{
    Task<IReadOnlyList<JobFeedTask>> Fetch(
        int batchSize,
        CancellationToken ct);
}