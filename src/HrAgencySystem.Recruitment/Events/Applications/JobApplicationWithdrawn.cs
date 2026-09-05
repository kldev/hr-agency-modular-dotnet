using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationWithdrawn(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt, 
    Guid AuthorId,
    UserSnapshot Author) : IJobApplicationEvent;