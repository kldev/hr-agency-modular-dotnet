using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.WorkerDocuments.Remove;

public static class RemoveWorkerDocumentHandler
{
    public const string UnknownDocumentMessage = "That document is not on this person's file.";

    public const string ReferencedByAuthorisationMessage =
        "This document is the proof behind a recorded permission. Remove the permission first.";

    [AggregateHandler]
    public static async Task<(WorkerDocumentRemoved, Wolverine.Marten.Events)> Handle(
        RemoveWorkerDocument command,
        Worker aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var document =
            aggregate.DocumentById(command.DocumentId)
            ?? throw new BusinessRuleException(UnknownDocumentMessage);

        // Removing the scan while the permit record stays would leave a permission claiming
        // evidence that is no longer there - the one state this file must never be able to reach.
        if (aggregate.Authorisations.Any(a => a.DocumentId == command.DocumentId))
            throw new BusinessRuleException(ReferencedByAuthorisationMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new WorkerDocumentRemoved(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.DocumentId,
            document.FileId,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
