namespace HrAgencySystem.Recruitment.Feeds.Application.ScheduleFeedTasks;

public interface IJobFeedScheduler
{
    Task ScheduleAsync(CancellationToken ct);
}