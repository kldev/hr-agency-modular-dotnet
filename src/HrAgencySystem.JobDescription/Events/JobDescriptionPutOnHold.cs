namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionPutOnHold(
    DateTimeOffset OccurredAt);