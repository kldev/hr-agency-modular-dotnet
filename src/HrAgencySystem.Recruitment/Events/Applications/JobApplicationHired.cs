using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationHired(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt,
    UserSnapshot Author
) : IJobApplicationEvent;
