using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Tasks.Application.Reopen;

/// <summary>
/// Takes a tick back - a mis-click on a one-click list has to be undoable. The opportunity keeps
/// its history entry: it records that the task was done at that time, which stays true.
/// </summary>
public static class ReopenTaskItemHandler
{
    [AggregateHandler]
    public static async Task<(TaskItemReopened, Wolverine.Marten.Events)> Handle(
        ReopenTaskItem command,
        TaskItem task,
        ITasksService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(task, command.OrganizationId, command.TaskId);
        TaskItemStatusPolicy.EnsureCanReopen(task);

        var reopenedBy = await service.GetUserAsync(command.ModifiedBy, task.OrganizationId, ct);

        var @event = new TaskItemReopened(task.Id, command.OrganizationId, reopenedBy, clock.UtcNow);

        return (@event, [@event]);
    }
}
