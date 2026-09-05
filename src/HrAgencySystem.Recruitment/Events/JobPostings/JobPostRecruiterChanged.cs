using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public sealed record JobPostRecruiterChanged(Guid JobPostId,
    UserSnapshot Recruiter,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;
