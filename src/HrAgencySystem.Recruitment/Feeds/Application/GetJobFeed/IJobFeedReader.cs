using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;

public interface IJobFeedReader
{
    Task<IReadOnlyList<JobPostProjection>> GetJobsFeed(Guid organizationId, CancellationToken ct);
}