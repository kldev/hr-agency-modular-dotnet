using HrAgencySystem.Recruitment.Feeds.Model;

namespace HrAgencySystem.Recruitment.Feeds.Port;

internal interface IJobFeedTaskRepository
{
    Task Save(JobFeedTask task,  CancellationToken ct);
    Task BatchSave(IReadOnlyList<JobFeedTask> tasks,  CancellationToken ct);
}