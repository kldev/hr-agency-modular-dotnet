namespace HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;

internal interface IJobFeedGenerator
{
    Task<JobFeedGenerator.JobFeedContent> GenerateAsync(Guid organizationId, CancellationToken ct);
}