namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionOpened(
    Guid JobDescriptionId,
    DateTimeOffset OccurredAt);