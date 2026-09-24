using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks.Contracts.IntegrationEvents;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Services;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Tasks.Application.Complete;

/// <summary>
/// Ticks a task off. When it belongs to a deal, the sales module is told, so the opportunity's
/// history shows the work - the message rides in <see cref="OutgoingMessages"/>, because the first
/// item of the tuple is the reply to the endpoint and is never cascaded.
/// </summary>
public static class CompleteTaskItemHandler
{
    [AggregateHandler]
    public static async Task<(TaskItemCompleted, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        CompleteTaskItem command,
        TaskItem task,
        ITasksService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(task, command.OrganizationId, command.TaskId);
        TaskItemStatusPolicy.EnsureCanComplete(task);

        var completedBy = await service.GetUserAsync(command.ModifiedBy, task.OrganizationId, ct);

        var @event = new TaskItemCompleted(
            task.Id,
            command.OrganizationId,
            task.Completions + 1,
            completedBy,
            clock.UtcNow
        );

        var messages = new OutgoingMessages();

        if (task.Opportunity is { } opportunity)
            messages.Add(
                new OpportunityTaskCompleted(
                    command.OrganizationId,
                    task.Id,
                    @event.Completion,
                    opportunity.Id,
                    task.Title,
                    completedBy.Id,
                    @event.CompletedAt
                )
            );

        return (@event, [@event], messages);
    }
}
