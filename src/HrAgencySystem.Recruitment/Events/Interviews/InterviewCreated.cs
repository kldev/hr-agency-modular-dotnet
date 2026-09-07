using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Interviews;

public sealed record InterviewCreated(
    Guid InterviewId,
    Guid OrganizationId,
    Guid JobApplicationId,
    Guid CandidateId,
    Guid CompanyId,
    DateTimeOffset ScheduleAt,
    string Timezone,
    UserSnapshot Interviewer,
    InterviewFormat Format,
    InterviewType InterviewType,
    string Note,
    UserSnapshot Author,
    DateTimeOffset OccurredAt) : IInterviewEvent;
