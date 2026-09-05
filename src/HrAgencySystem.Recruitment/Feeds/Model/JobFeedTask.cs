namespace HrAgencySystem.Recruitment.Feeds.Model;

internal sealed record JobFeedTask(
    Guid Id,
    Guid OrganizationId,
    JobFeedTaskStatus Status,
    int Attempts,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage);
