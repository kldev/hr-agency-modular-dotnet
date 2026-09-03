using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobApplication;

public sealed record JobApplicationScreeningStarted(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt, 
    Guid AuthorId,
    UserSnapshot Author): IJobApplicationEvent;