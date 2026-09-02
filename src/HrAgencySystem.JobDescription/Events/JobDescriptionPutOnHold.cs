namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionPutOnHold(
    Guid JobDescriptionId,
    DateTimeOffset OccurredAt);