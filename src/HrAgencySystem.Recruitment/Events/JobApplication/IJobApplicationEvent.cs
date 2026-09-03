using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobApplication;

public interface IJobApplicationEvent
{
    Guid JobApplicationId { get; }
    DateTimeOffset OccurredAt { get; }
    Guid AuthorId { get; }
    UserSnapshot Author { get; }
}