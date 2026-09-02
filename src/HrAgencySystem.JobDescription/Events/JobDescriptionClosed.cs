using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionClosed(
    Guid JobDescriptionId,
    UserSnapshot ModifiedBy,
    DateTimeOffset OccurredAt);