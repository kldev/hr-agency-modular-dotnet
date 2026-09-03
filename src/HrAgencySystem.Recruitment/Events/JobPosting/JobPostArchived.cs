using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostArchived(
    Guid JobPostId,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;