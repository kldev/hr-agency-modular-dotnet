using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Tasks.Domain;

namespace HrAgencySystem.Tasks.Events;

public sealed record TaskItemCreated(
    Guid TaskId,
    Guid OrganizationId,
    string Title,
    string? Description,
    DateTimeOffset DueAt,
    TaskPriority Priority,
    CompanySnapshot Company,
    TaskOpportunity? Opportunity,
    UserSnapshot Assignee,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);

/// <summary>
/// One event for everything the task drawer edits - the drawer saves them together, so splitting
/// them would only produce several events for one decision. The company is not here: a task for
/// another company is another task.
/// </summary>
public sealed record TaskItemUpdated(
    Guid TaskId,
    Guid OrganizationId,
    string Title,
    string? Description,
    DateTimeOffset DueAt,
    TaskPriority Priority,
    TaskOpportunity? Opportunity,
    UserSnapshot Assignee,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record TaskItemCompleted(
    Guid TaskId,
    Guid OrganizationId,
    int Completion,
    UserSnapshot CompletedBy,
    DateTimeOffset CompletedAt
);

public sealed record TaskItemReopened(
    Guid TaskId,
    Guid OrganizationId,
    UserSnapshot ReopenedBy,
    DateTimeOffset ReopenedAt
);
