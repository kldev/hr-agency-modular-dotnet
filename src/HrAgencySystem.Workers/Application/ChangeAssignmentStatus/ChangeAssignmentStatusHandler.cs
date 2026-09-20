using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.ChangeAssignmentStatus;

public static class ChangeAssignmentStatusHandler
{
    public const string TransitionNotAllowedMessage =
        "This assignment status change is not allowed.";

    public const string EndsBeforeStartMessage =
        "An assignment cannot end before the day it started.";

    public const string WorkerNotReadyMessage =
        "This person is not through the pipeline yet, so the assignment cannot start.";

    [AggregateHandler]
    public static async Task<(AssignmentStatusChanged, Wolverine.Marten.Events)> Handle(
        ChangeAssignmentStatus command,
        Domain.Assignment aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (!AssignmentStatusChangePolicy.Allow(aggregate.Status, command.Status))
            throw new BusinessRuleException(TransitionNotAllowedMessage);

        if (command.Status is AssignmentStatus.Active)
            await EnsureWorkerMayStart(aggregate, service, ct);

        var endsOn = ReadEndsOn(command, aggregate);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new AssignmentStatusChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.WorkerId.Value,
            aggregate.Status,
            command.Status,
            endsOn,
            ReadReason(command),
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// Planning somebody in is allowed while their paperwork runs; starting them is not. The check
    /// is here rather than in the policy because it needs the person, whom a transition graph
    /// cannot see.
    /// </summary>
    private static async Task EnsureWorkerMayStart(
        Domain.Assignment assignment,
        IWorkersService service,
        CancellationToken ct
    )
    {
        var worker = await service.GetWorkerAsync(
            assignment.OrganizationId,
            assignment.WorkerId.Value,
            ct
        );

        if (!WorkerStatusChangePolicy.MayStartWork(worker.Status))
            throw new BusinessRuleException(WorkerNotReadyMessage);
    }

    /// <summary>
    /// Ending an assignment writes the date it ended; anything else keeps what was planned. An end
    /// date is not asked for when a posting merely goes live.
    /// </summary>
    private static DateOnly? ReadEndsOn(
        ChangeAssignmentStatus command,
        Domain.Assignment assignment
    )
    {
        if (!AssignmentStatusChangePolicy.IsFinal(command.Status))
            return assignment.EndsOn;

        var endsOn = command.EndsOn ?? assignment.EndsOn;

        // Somebody who never started has no last day, and inventing one would put them on the
        // calendar for a period they were not there.
        if (command.Status is AssignmentStatus.DidNotStart)
            return endsOn;

        if (endsOn is not null && endsOn < assignment.StartsOn)
            throw new ValidationException(EndsBeforeStartMessage);

        return endsOn;
    }

    private static string ReadReason(ChangeAssignmentStatus command)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
            return "";

        var (reason, error) = ShortNote.TryCreate(command.Reason);

        return error is not null ? throw new ValidationException(error) : reason!.Value;
    }
}
