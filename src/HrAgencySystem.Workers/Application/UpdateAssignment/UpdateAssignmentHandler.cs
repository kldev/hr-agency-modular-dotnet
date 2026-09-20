using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Application.PlanAssignment;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.UpdateAssignment;

/// <summary>
/// Corrects the position or the period of a posting that is still running. What it deliberately
/// cannot touch is the project, the person and the engagement type - moving somebody is a new
/// assignment, and changing how they work changes which obligations apply, which would strand the
/// ones already recorded.
/// </summary>
public static class UpdateAssignmentHandler
{
    public const string AlreadyFinishedMessage =
        "This assignment has finished, so its terms cannot be changed.";

    [AggregateHandler]
    public static async Task<(AssignmentUpdated, Wolverine.Marten.Events)> Handle(
        UpdateAssignment command,
        Domain.Assignment aggregate,
        IWorkersService service,
        IAssignmentsQueryRepository assignments,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (AssignmentStatusChangePolicy.IsFinal(aggregate.Status))
            throw new BusinessRuleException(AlreadyFinishedMessage);

        var position = ReadPosition(command);

        var project = await service.GetProjectAsync(
            OrganizationId.From(command.OrganizationId),
            aggregate.Project.ProjectId,
            ct
        );

        if (!project.Covers(command.StartsOn, command.EndsOn))
            throw new BusinessRuleException(PlanAssignmentHandler.OutsideProjectPeriodMessage);

        // The person's other postings still may not overlap - this one excluded, since moving its
        // own dates is the whole point.
        if (
            await assignments.HasOverlappingAssignment(
                OrganizationId.From(command.OrganizationId),
                aggregate.WorkerId.Value,
                command.StartsOn,
                command.EndsOn,
                aggregate.Id.Value,
                ct
            )
        )
            throw new BusinessRuleException(PlanAssignmentHandler.OverlapsAnotherAssignmentMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new AssignmentUpdated(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.WorkerId.Value,
            position.Value,
            command.StartsOn,
            command.EndsOn,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private static PersonJobTitle ReadPosition(UpdateAssignment command)
    {
        var errors = new List<string>();

        var (position, positionError) = PersonJobTitle.TryCreate(command.Position, true);
        if (positionError is not null)
            errors.Add(positionError);

        if (command.EndsOn is not null && command.EndsOn < command.StartsOn)
            errors.Add(PlanAssignmentHandler.EndsBeforeStartMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return position!;
    }
}
