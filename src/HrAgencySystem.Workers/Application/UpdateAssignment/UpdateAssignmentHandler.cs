using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.PlanAssignment;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine;
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
    public static async Task<(AssignmentUpdated, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        UpdateAssignment command,
        Assignment aggregate,
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

        ReadPeriod(command);

        var project = await service.GetProjectAsync(
            OrganizationId.From(command.OrganizationId),
            aggregate.Project.ProjectId,
            ct
        );

        if (!project.Covers(command.StartsOn, command.EndsOn))
            throw new BusinessRuleException(PlanAssignmentHandler.OutsideProjectPeriodMessage);

        // The project cannot be corrected here, so the role is resolved inside the one the posting
        // already names - a role from anywhere else is simply not a role on this posting.
        var position = await service.GetPositionAsync(
            OrganizationId.From(command.OrganizationId),
            aggregate.Project.ProjectId,
            command.PositionId,
            ct
        );

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
            AssignmentPosition.From(position),
            command.StartsOn,
            command.EndsOn,
            user,
            clock.UtcNow
        );

        var messages = new OutgoingMessages();

        // Correcting the role moves the seat rather than adding one. Whether this should be a
        // correction at all is the open question on the plan - either way the counts may not be
        // left describing a role this person no longer sits on.
        if (aggregate.Position.PositionId != command.PositionId)
        {
            messages.Add(
                new AssignmentPositionUnstaffed(
                    aggregate.OrganizationId.Value,
                    aggregate.Project.ProjectId,
                    aggregate.Position.PositionId,
                    aggregate.Id.Value
                )
            );
            messages.Add(
                new AssignmentPositionStaffed(
                    aggregate.OrganizationId.Value,
                    aggregate.Project.ProjectId,
                    command.PositionId,
                    aggregate.Id.Value
                )
            );
        }

        return (@event, [@event], messages);
    }

    private static void ReadPeriod(UpdateAssignment command)
    {
        if (command.EndsOn is not null && command.EndsOn < command.StartsOn)
            throw new ValidationException([PlanAssignmentHandler.EndsBeforeStartMessage]);
    }
}
