using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Tasks.Application.Update;

public static class UpdateTaskItemHandler
{
    [AggregateHandler]
    public static async Task<(TaskItemUpdated, Wolverine.Marten.Events)> Handle(
        UpdateTaskItem command,
        TaskItem task,
        ITasksService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(task, command.OrganizationId, command.TaskId);
        TaskItemStatusPolicy.EnsureCanChange(task);

        var input = TaskItemInputValidator.Validate(command);

        // The same deal is kept as it was saved; only a different one is looked up again.
        var opportunity = command.OpportunityId switch
        {
            null => null,
            { } id when id == task.Opportunity?.Id => task.Opportunity,
            { } id => await service.GetOpportunityAsync(id, task.Company.Id, task.OrganizationId, ct),
        };

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, task.OrganizationId, ct);

        var assignee =
            command.AssigneeId == task.Assignee.Id ? task.Assignee
            : command.AssigneeId == modifiedBy.Id ? modifiedBy
            : await service.GetUserAsync(command.AssigneeId, task.OrganizationId, ct);

        var @event = new TaskItemUpdated(
            task.Id,
            command.OrganizationId,
            input.Title,
            input.Description,
            input.DueAt,
            input.Priority,
            opportunity,
            assignee,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
