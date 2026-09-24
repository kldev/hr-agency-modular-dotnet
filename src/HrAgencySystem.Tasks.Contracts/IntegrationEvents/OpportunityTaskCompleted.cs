namespace HrAgencySystem.Tasks.Contracts.IntegrationEvents;

/// <summary>
/// A task that belongs to a sales opportunity was done. Sent only for a task with an opportunity -
/// the sales module turns it into an entry of the opportunity's history.
/// <para>
/// <see cref="Completion"/> counts the times this task was completed: a task reopened and done again
/// is a second entry, while the same message delivered twice (at least once) must stay one.
/// </para>
/// </summary>
public sealed record OpportunityTaskCompleted(
    Guid OrganizationId,
    Guid TaskId,
    int Completion,
    Guid OpportunityId,
    string Title,
    Guid CompletedById,
    DateTimeOffset CompletedAt
);
