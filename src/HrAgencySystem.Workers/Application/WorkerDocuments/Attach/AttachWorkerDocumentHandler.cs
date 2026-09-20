using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.WorkerDocuments.Attach;

public static class AttachWorkerDocumentHandler
{
    [AggregateHandler]
    public static async Task<(WorkerDocumentAttached, Wolverine.Marten.Events)> Handle(
        AttachWorkerDocument command,
        Worker aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var (fileName, note) = WorkerDocumentFactory.Create(
            command.FileName,
            command.DocumentDate,
            command.ValidUntil,
            command.Note
        );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var document = new WorkerDocument(
            Guid.NewGuid(),
            command.Category,
            command.FileId,
            fileName,
            command.ContentType,
            command.Size,
            command.DocumentDate,
            command.ValidUntil,
            note
        );

        var @event = new WorkerDocumentAttached(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            document,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
