using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Projections;

namespace HrAgencySystem.Tasks.Application.Port;

/// <param name="AssigneeId">Whose list it is.</param>
/// <param name="CompanyId">One company only, or every company when left out.</param>
public sealed record TaskBoardQuery(Guid AssigneeId, TaskRange Range, DateTimeOffset Now, Guid? CompanyId);

public interface ITaskItemsQueryRepository
{
    /// <summary>
    /// The size of each section before it is cut. A person's own tasks in a month are a short
    /// list; the limit is a safety net against a runaway fixture, not a paging scheme.
    /// </summary>
    public const int SectionLimit = 200;

    /// <summary>
    /// Open tasks due before the end of the range - including overdue ones, which would otherwise
    /// drop out of sight exactly when they matter - and tasks done within it.
    /// </summary>
    Task<TaskBoard> GetBoard(OrganizationId organizationId, TaskBoardQuery query, CancellationToken ct);

    Task<TaskItemProjection?> GetTask(OrganizationId organizationId, Guid taskId, CancellationToken ct);
}

public sealed record TaskBoard(
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyList<TaskItemRow> Active,
    IReadOnlyList<TaskItemRow> Completed
);

public sealed record TaskItemRef(Guid Id, string Name);

public sealed record TaskItemRow(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset DueAt,
    TaskPriority Priority,
    TaskItemStatus Status,
    bool IsOverdue,
    TaskItemRef Company,
    TaskItemRef? Opportunity,
    Guid AssigneeId,
    string AssigneeName,
    DateTimeOffset? CompletedAt
)
{
    public static TaskItemRow From(TaskItemProjection task, DateTimeOffset now) =>
        new(
            task.Id,
            task.Title,
            task.Description,
            task.DueAt,
            task.Priority,
            task.Status,
            task.Status == TaskItemStatus.Open && task.DueAt < now,
            new TaskItemRef(task.CompanyId, task.CompanyName),
            task.OpportunityId is { } id ? new TaskItemRef(id, task.OpportunityTitle ?? "") : null,
            task.AssigneeId,
            task.Assignee.Fullname,
            task.CompletedAt
        );
}
