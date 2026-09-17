using HrAgencySystem.Feeds.ReadModel;

namespace HrAgencySystem.Feeds.Application.GetJobFeed;

public interface IJobFeedReader
{
    Task<IReadOnlyList<JobPostFeedRow>> GetJobsFeed(Guid organizationId, CancellationToken ct);
}
