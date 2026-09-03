using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public interface IJobPostingEvent
{
    Guid JobPostId { get; }
    DateTimeOffset OccurredAt { get; }
    Guid AuthorId { get; }
    UserSnapshot Author { get; }
}