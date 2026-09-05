namespace HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;

public interface IJobFeedProcessor
{
    Task ProcessBatch(CancellationToken ct);
}