using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Interviews;

public sealed record InterviewFormatChanged(
    Guid InterviewId,
    InterviewFormat OldFormat,
    InterviewFormat NewFormat,
    UserSnapshot Author,
    DateTimeOffset OccurredAt
) : IInterviewEvent;