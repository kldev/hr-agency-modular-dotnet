using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Interviews;

public interface IInterviewEvent
{
    Guid InterviewId { get; }
    DateTimeOffset OccurredAt { get; }
    UserSnapshot Author { get; }
}