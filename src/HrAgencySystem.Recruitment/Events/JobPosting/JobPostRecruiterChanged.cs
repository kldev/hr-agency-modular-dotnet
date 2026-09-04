using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostRecruiterChanged(Guid JobPostId,
    UserSnapshot Recruiter,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;
