using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionOpened(
    Guid JobDescriptionId,
    UserSnapshot ModifiedBy,
    DateTimeOffset OccurredAt);