using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.WorkerDocuments.UpdateMetadata;

public static class UpdateWorkerDocumentMetadataHandler
{
    public const string UnknownDocumentMessage = "That document is not on this person's file.";

    [AggregateHandler]
    public static async Task<(WorkerDocumentMetadataChanged, Wolverine.Marten.Events)> Handle(
        UpdateWorkerDocumentMetadata command,
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

        var (_, note) = WorkerDocumentFactory.Create(
            document.FileName,
            command.DocumentDate,
            command.ValidUntil,
            command.Note
        );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        // The file itself never changes here. Replacing a document means attaching a new one.
        var changed = document with
        {
            Category = command.Category,
            DocumentDate = command.DocumentDate,
            ValidUntil = command.ValidUntil,
            Note = note,
        };

        var @event = new WorkerDocumentMetadataChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            changed,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
