using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPosting;

public interface IJobPostEvent
{
    Guid JobPostId { get; }
    DateTimeOffset OccurredAt { get; }
    Guid AuthorId { get; }
    UserSnapshot Author { get; }
}