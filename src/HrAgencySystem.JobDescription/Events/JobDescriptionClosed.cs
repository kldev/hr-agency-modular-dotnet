namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionClosed(
    Guid JobDescriptionId,
    DateTimeOffset OccurredAt);