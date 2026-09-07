using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationScreeningStarted(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt, 
    UserSnapshot Author): IJobApplicationEvent;