using HrAgencySystem.Feeds.Model;

namespace HrAgencySystem.Feeds.Port;

internal interface IJobFeedTaskRepository
{
    Task Save(JobFeedTask task,  CancellationToken ct);
    Task BatchSave(IReadOnlyList<JobFeedTask> tasks,  CancellationToken ct);
    Task MarkFailed(Guid id, string errorMessage, CancellationToken ct);
    Task MarkCompleted(Guid id, CancellationToken ct);
}