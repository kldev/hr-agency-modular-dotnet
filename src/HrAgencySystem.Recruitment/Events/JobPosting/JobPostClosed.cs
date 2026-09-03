using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostClosed(
    Guid JobPostId,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;