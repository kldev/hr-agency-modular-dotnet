using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Interviews;

public sealed record InterviewStatusChanged(
    Guid InterviewId,
    InterviewStatus OldStatus,
    InterviewStatus NewStatus,
    UserSnapshot Author,
    DateTimeOffset OccurredAt
    ) : IInterviewEvent;