using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Interviews;

public sealed record InterviewRescheduled(
    Guid InterviewId,
    Guid OrganizationId,
    DateTimeOffset ScheduleAt,
    string Timezone,
    string Note,
    UserSnapshot Author,
    DateTimeOffset OccurredAt) : IInterviewEvent;
