using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionCancelled(
    Guid JobDescriptionId,
    UserSnapshot ModifiedBy,
    DateTimeOffset OccurredAt);