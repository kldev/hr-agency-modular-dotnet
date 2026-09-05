using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public interface IJobPostEvent
{
    Guid JobPostId { get; }
    DateTimeOffset OccurredAt { get; }
    UserSnapshot Author { get; }
}