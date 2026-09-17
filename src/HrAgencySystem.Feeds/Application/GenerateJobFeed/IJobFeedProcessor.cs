namespace HrAgencySystem.Feeds.Application.GenerateJobFeed;

public interface IJobFeedProcessor
{
    Task ProcessBatch(CancellationToken ct);
}