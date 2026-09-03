using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostPublished(
    Guid JobPostId,
    DateTimeOffset OccurredAt,
    Guid AuthorId,
    UserSnapshot Author) : IJobPostEvent;