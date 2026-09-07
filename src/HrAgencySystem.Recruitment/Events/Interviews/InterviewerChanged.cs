using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Interviews;

public sealed record InterviewerChanged( Guid InterviewId,
    UserSnapshot PreviousInterview,
    UserSnapshot NewInterviewer,
    UserSnapshot Author,
    DateTimeOffset OccurredAt
    ) : IInterviewEvent;
