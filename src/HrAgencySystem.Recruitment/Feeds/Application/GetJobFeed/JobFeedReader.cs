using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Infrastructure.Query;
using HrAgencySystem.Recruitment.Projections;
using Marten;

namespace HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;

sealed class JobFeedReader(IQuerySession session) : IJobFeedReader
{
    public async Task<IReadOnlyList<JobPostProjection>> GetJobsFeed(Guid organizationId, CancellationToken ct)
    {
        var jobs = await session.Query<JobPostProjection>().WithStatuses([JobPostStatus.Published])
            .WithOrganizationId(organizationId).ToListAsync(ct);
        return jobs;
    }
}