namespace HrAgencySystem.Feeds.Application.ScheduleFeedTasks;

public interface IJobFeedScheduler
{
    Task ScheduleAsync(CancellationToken ct);
}
