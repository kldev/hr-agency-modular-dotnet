namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionCancelled(
    Guid JobDescriptionId,
    DateTimeOffset OccurredAt);