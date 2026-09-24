using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;

namespace HrAgencySystem.Tasks.Projections;

/// <summary>
/// One row per task. The assignee, the company and the two dates the lists are cut by are flattened
/// into columns, so "my open tasks due before Sunday" is an index scan, not a document search.
/// </summary>
public sealed record TaskItemProjection(
    Guid Id,
    Guid OrganizationId,
    string Title,
    string? Description,
    DateTimeOffset DueAt,
    TaskPriority Priority,
    TaskItemStatus Status,
    Guid CompanyId,
    string CompanyName,
    Guid? OpportunityId,
    string? OpportunityTitle,
    Guid AssigneeId,
    UserSnapshot Assignee,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    UserSnapshot? CompletedBy
)
{
    public static TaskItemProjection Create(TaskItemCreated @event) =>
        new(
            @event.TaskId,
            @event.OrganizationId,
            @event.Title,
            @event.Description,
            @event.DueAt,
            @event.Priority,
            TaskItemStatus.Open,
            @event.Company.Id,
            @event.Company.Name,
            @event.Opportunity?.Id,
            @event.Opportunity?.Title,
            @event.Assignee.Id,
            @event.Assignee,
            @event.CreatedBy,
            @event.CreatedAt,
            null,
            null
        );

    public TaskItemProjection Apply(TaskItemUpdated @event) =>
        this with
        {
            Title = @event.Title,
            Description = @event.Description,
            DueAt = @event.DueAt,
            Priority = @event.Priority,
            OpportunityId = @event.Opportunity?.Id,
            OpportunityTitle = @event.Opportunity?.Title,
            AssigneeId = @event.Assignee.Id,
            Assignee = @event.Assignee,
        };

    public TaskItemProjection Apply(TaskItemCompleted @event) =>
        this with
        {
            Status = TaskItemStatus.Done,
            CompletedAt = @event.CompletedAt,
            CompletedBy = @event.CompletedBy,
        };

    public TaskItemProjection Apply(TaskItemReopened _) =>
        this with
        {
            Status = TaskItemStatus.Open,
            CompletedAt = null,
            CompletedBy = null,
        };
}
