using HrAgencySystem.Recruitment.Config;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Infrastructure.Query;
using HrAgencySystem.Recruitment.Projections;
using Marten;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;

// ReSharper disable once ClassNeverInstantiated.Global
sealed class JobFeedReader(IQuerySession session, IOptions<RecruitmentConfig> config) : IJobFeedReader
{
    public async Task<IReadOnlyList<JobPostProjection>> GetJobsFeed(Guid organizationId, CancellationToken ct)
    {

        if (string.IsNullOrEmpty(config.Value.FeedUrl))
            throw new ArgumentException("FeedsUrl must be provided. Check AppSettings.json -> Application -> FeedUrl value.");
        

        var jobs = await session.Query<JobPostProjection>().WithStatuses([JobPostStatus.Published])
            .WithOrganizationId(organizationId).ToListAsync(ct);

        var updatedJobs = jobs.Select(z => z.UpdatePostSlug(config.Value.FeedUrl)).ToList();
        
        return updatedJobs;
    }
}