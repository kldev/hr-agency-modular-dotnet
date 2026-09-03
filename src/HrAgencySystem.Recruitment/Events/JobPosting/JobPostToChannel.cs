using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public sealed record JobPostToChannel(
    Guid JobPostId,
    JobPostingChannel Channel,
    DateTimeOffset OccurredAt,
    Guid AuthorId,
    UserSnapshot Author) : IJobPostEvent;
