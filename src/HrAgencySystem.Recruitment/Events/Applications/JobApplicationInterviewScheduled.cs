using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationInterviewScheduled(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt,
    Guid AuthorId,
    UserSnapshot Author,
    Guid InterviewId) : IJobApplicationEvent;