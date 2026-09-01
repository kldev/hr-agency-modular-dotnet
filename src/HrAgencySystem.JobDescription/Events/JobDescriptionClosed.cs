namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionClosed(
    DateTimeOffset OccurredAt);