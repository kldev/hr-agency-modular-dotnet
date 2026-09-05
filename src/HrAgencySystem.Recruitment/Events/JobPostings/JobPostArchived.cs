using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public sealed record JobPostArchived(
    Guid JobPostId,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;