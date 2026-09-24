using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Marten;

namespace HrAgencySystem.Workers.Application.PlanAssignment;

public static class PlanAssignmentHandler
{
    public const string EndsBeforeStartMessage =
        "The end date cannot be earlier than the start date.";

    public const string OutsideProjectPeriodMessage =
        "The period falls outside the project's own period.";

    public const string OverlapsAnotherAssignmentMessage =
        "This person is already assigned over part of that period.";

    public const string WorkerNotAvailableMessage =
        "This person has left, so they cannot be assigned to anything.";

    public static async Task<(AssignmentPlanned, AssignmentPositionStaffed)> Handle(
        PlanAssignment command,
        IWorkersService service,
        IAssignmentsQueryRepository assignments,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);

        ReadPeriod(command);

        await service.ValidateOrganization(command.OrganizationId, ct);

        var worker = await service.GetWorkerAsync(organizationId, command.WorkerId, ct);

        // Planning ahead while the paperwork runs is normal and allowed; putting somebody who has
        // left on next month's crew is not.
        if (!WorkerStatusChangePolicy.MayBePlanned(worker.Status))
            throw new BusinessRuleException(WorkerNotAvailableMessage);

        var project = await service.GetProjectAsync(organizationId, command.ProjectId, ct);

        if (!project.Covers(command.StartsOn, command.EndsOn))
            throw new BusinessRuleException(OutsideProjectPeriodMessage);

        // Resolved inside the project, not looked up on its own: a role from another delivery would
        // freeze a name that has nothing to do with where this person actually works, and asking
        // for it this way is what makes that impossible rather than merely checked.
        var position = await service.GetPositionAsync(
            organizationId,
            command.ProjectId,
            command.PositionId,
            ct
        );

        // Nobody works two positions at once. Read from the projection, with the race that implies -
        // see IAssignmentsQueryRepository.
        if (
            await assignments.HasOverlappingAssignment(
                organizationId,
                command.WorkerId,
                command.StartsOn,
                command.EndsOn,
                null,
                ct
            )
        )
            throw new BusinessRuleException(OverlapsAnotherAssignmentMessage);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var assignmentId = AssignmentId.New();

        var @event = new AssignmentPlanned(
            assignmentId.Value,
            organizationId.Value,
            worker.Id.Value,
            worker.FullName,
            new ProjectPlacementSnapshot(
                project.Id,
                project.Name,
                project.CompanyId,
                project.CompanyName,
                project.DeliveringEntityId,
                project.DeliveringEntityName,
                project.WorkCountry
            ),
            command.EngagementType,
            AssignmentPosition.From(position),
            command.StartsOn,
            command.EndsOn,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<Assignment>(assignmentId.Value, @event);

        // The role is taken from the moment somebody is planned onto it, not from the day they fly
        // out: a seat held for next month is not a seat anybody else can be offered. Cascaded, so
        // the caller still gets the domain event back and the counting stays in the other module.
        return (
            @event,
            new AssignmentPositionStaffed(
                organizationId.Value,
                project.Id,
                position.Id,
                assignmentId.Value
            )
        );
    }

    /// <summary>
    /// The only thing left to validate on the way in. The role used to be free text validated here;
    /// now it is an id resolved against the project, which is a rule rather than a format.
    /// </summary>
    private static void ReadPeriod(PlanAssignment command)
    {
        if (command.EndsOn is not null && command.EndsOn < command.StartsOn)
            throw new ValidationException([EndsBeforeStartMessage]);
    }
}
