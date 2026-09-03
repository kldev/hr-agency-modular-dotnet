using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostPublished(
    Guid JobPostId,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;