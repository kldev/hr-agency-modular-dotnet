using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationWithdrawn(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt, 
    UserSnapshot Author) : IJobApplicationEvent;