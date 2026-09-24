using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Tasks.Events;

namespace HrAgencySystem.Tasks.Domain;

/// <summary>
/// Something a person at the agency has to do by a given time, for a client company - "call the
/// HR manager", "send the proposal". Always for a company, optionally within one of its deals.
/// <para>
/// Not a sales follow-up: a follow-up is the next step noted on an opportunity, a task is work with
/// an owner, a priority and a state, done or not. No decisions here - <c>Apply</c> replays what
/// happened, the rules live in <see cref="TaskItemStatusPolicy"/> and in the handlers.
/// </para>
/// </summary>
public sealed class TaskItem : IOrganizationDomain
{
    private TaskItem() { }

    public static TaskItem Empty() => new();

    public Guid Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }

    public string Title { get; private set; } = "";
    public string? Description { get; private set; }
    public DateTimeOffset DueAt { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskItemStatus Status { get; private set; }

    public CompanySnapshot Company { get; private set; } = null!;
    public TaskOpportunity? Opportunity { get; private set; }
    public UserSnapshot Assignee { get; private set; } = null!;

    /// <summary>How many times it was done; reopening does not take one back.</summary>
    public int Completions { get; private set; }

    public void Apply(TaskItemCreated @event)
    {
        Id = @event.TaskId;
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Title = @event.Title;
        Description = @event.Description;
        DueAt = @event.DueAt;
        Priority = @event.Priority;
        Status = TaskItemStatus.Open;
        Company = @event.Company;
        Opportunity = @event.Opportunity;
        Assignee = @event.Assignee;
    }

    public void Apply(TaskItemUpdated @event)
    {
        Title = @event.Title;
        Description = @event.Description;
        DueAt = @event.DueAt;
        Priority = @event.Priority;
        Opportunity = @event.Opportunity;
        Assignee = @event.Assignee;
    }

    public void Apply(TaskItemCompleted @event)
    {
        Status = TaskItemStatus.Done;
        Completions = @event.Completion;
    }

    public void Apply(TaskItemReopened _)
    {
        Status = TaskItemStatus.Open;
    }
}
