namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionCancelled(
    DateTimeOffset OccurredAt);