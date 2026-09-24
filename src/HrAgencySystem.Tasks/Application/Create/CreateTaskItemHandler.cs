using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Services;
using Marten;

namespace HrAgencySystem.Tasks.Application.Create;

public static class CreateTaskItemHandler
{
    public static async Task<TaskItemCreated> Handle(
        CreateTaskItem command,
        ITasksService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var input = TaskItemInputValidator.Validate(command);
        var organizationId = OrganizationId.From(command.OrganizationId);

        var company = await service.GetCompanyAsync(command.CompanyId, organizationId, ct);

        var opportunity = command.OpportunityId is { } opportunityId
            ? await service.GetOpportunityAsync(opportunityId, company.Id, organizationId, ct)
            : null;

        var createdBy = await service.GetUserAsync(command.CreatedBy, organizationId, ct);

        var assignee =
            command.AssigneeId is { } assigneeId && assigneeId != createdBy.Id
                ? await service.GetUserAsync(assigneeId, organizationId, ct)
                : createdBy;

        var @event = new TaskItemCreated(
            Guid.CreateVersion7(),
            command.OrganizationId,
            input.Title,
            input.Description,
            input.DueAt,
            input.Priority,
            company,
            opportunity,
            assignee,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<TaskItem>(@event.TaskId, @event);

        return @event;
    }
}
