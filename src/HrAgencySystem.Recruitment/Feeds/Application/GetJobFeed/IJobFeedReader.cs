using HrAgencySystem.Recruitment.Feeds.ReadModel;

namespace HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;

public interface IJobFeedReader
{
    Task<IReadOnlyList<JobPostFeedRow>> GetJobsFeed(Guid organizationId, CancellationToken ct);
}
