using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public sealed record JobPostClosed(
    Guid JobPostId,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;