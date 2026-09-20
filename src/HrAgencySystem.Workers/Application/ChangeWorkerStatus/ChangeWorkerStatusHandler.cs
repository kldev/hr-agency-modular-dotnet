using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.ChangeWorkerStatus;

public static class ChangeWorkerStatusHandler
{
    public const string TransitionNotAllowedMessage = "This worker status change is not allowed.";

    public const string LegalisationNotRequiredMessage =
        "This person holds free movement rights, so there is nothing for legalisation to do.";

    public const string IdentityDocumentExpiredMessage =
        "The identity document on file has expired, so the person cannot be marked as employed.";

    [AggregateHandler]
    public static async Task<(WorkerStatusChanged, Wolverine.Marten.Events)> Handle(
        ChangeWorkerStatus command,
        Worker aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var requiresLegalisation = aggregate.RequiresLegalisation;

        if (!WorkerStatusChangePolicy.Allow(aggregate.Status, command.Status, requiresLegalisation))
            throw new BusinessRuleException(
                // Sending an EEA national to legalisation is the one refusal worth naming, because
                // "not allowed" would read as a broken screen rather than as "they do not need it".
                command.Status is WorkerStatus.Legalisation
                && !requiresLegalisation
                    ? LegalisationNotRequiredMessage
                    : TransitionNotAllowedMessage
            );

        if (command.Status is WorkerStatus.Employed)
            EnsureReadyToBeEmployed(aggregate, DateOnly.FromDateTime(clock.UtcNow.UtcDateTime));

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new WorkerStatusChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.Status,
            command.Status,
            ReadReason(command),
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// The one thing that has to be true before somebody counts as employed, checked here rather
    /// than in the policy because it needs the date and the file, neither of which a transition
    /// graph can see. The permits themselves are not checked: a permit is per country and per
    /// period, so whether one covers the work is a question the assignment answers.
    /// </summary>
    private static void EnsureReadyToBeEmployed(Worker worker, DateOnly today)
    {
        if (worker.IdentityDocument.ValidUntil is { } validUntil && validUntil < today)
            throw new BusinessRuleException(IdentityDocumentExpiredMessage);
    }

    private static string ReadReason(ChangeWorkerStatus command)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
            return "";

        var (reason, error) = ShortNote.TryCreate(command.Reason);

        return error is not null ? throw new ValidationException(error) : reason!.Value;
    }
}
