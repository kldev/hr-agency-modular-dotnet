namespace HrAgencySystem.Recruitment.Feeds.Model;

internal sealed record JobFeedTask(
    Guid Id,
    Guid OrganizationId,
    JobFeedTaskStatus Status,
    int Attempts,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage)
{
    internal static JobFeedTask Create(Guid organizationId, DateTimeOffset createdAt)
        => new(Guid.NewGuid(), organizationId,
            JobFeedTaskStatus.Pending,
            0, createdAt, null, null, null);
}
