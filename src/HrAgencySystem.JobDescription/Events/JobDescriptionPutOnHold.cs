using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionPutOnHold(
    Guid JobDescriptionId,
    UserSnapshot ModifiedBy,
    DateTimeOffset OccurredAt);