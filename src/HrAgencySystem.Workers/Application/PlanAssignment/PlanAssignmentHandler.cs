using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Application.Port;
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

    public static async Task<AssignmentPlanned> Handle(
        PlanAssignment command,
        IWorkersService service,
        IAssignmentsQueryRepository assignments,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var position = ReadPosition(command);

        await service.ValidateOrganization(command.OrganizationId, ct);

        var worker = await service.GetWorkerAsync(organizationId, command.WorkerId, ct);

        // Planning ahead while the paperwork runs is normal and allowed; putting somebody who has
        // left on next month's crew is not.
        if (!WorkerStatusChangePolicy.MayBePlanned(worker.Status))
            throw new BusinessRuleException(WorkerNotAvailableMessage);

        var project = await service.GetProjectAsync(organizationId, command.ProjectId, ct);

        if (!project.Covers(command.StartsOn, command.EndsOn))
            throw new BusinessRuleException(OutsideProjectPeriodMessage);

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
            position.Value,
            command.StartsOn,
            command.EndsOn,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<Domain.Assignment>(assignmentId.Value, @event);

        return @event;
    }

    private static PersonJobTitle ReadPosition(PlanAssignment command)
    {
        var errors = new List<string>();

        var (position, positionError) = PersonJobTitle.TryCreate(command.Position, true);
        if (positionError is not null)
            errors.Add(positionError);

        if (command.EndsOn is not null && command.EndsOn < command.StartsOn)
            errors.Add(EndsBeforeStartMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return position!;
    }
}
