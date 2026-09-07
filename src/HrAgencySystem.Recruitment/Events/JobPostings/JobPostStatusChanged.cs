using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public sealed record JobPostStatusChanged(
    Guid JobPostId,
    Guid CompanyId,
    Guid OrganizationId,
    JobPostStatus OldStatus,
    JobPostStatus NewStatus,
    DateTimeOffset OccurredAt,
    UserSnapshot Author) : IJobPostEvent;

