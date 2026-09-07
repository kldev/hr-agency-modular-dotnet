using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public interface IJobApplicationEvent
{
    Guid JobApplicationId { get; }
    DateTimeOffset OccurredAt { get; }
    UserSnapshot Author { get; }
}