using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.WorkerDocuments;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.AssignmentDocuments.Attach;

public static class AttachAssignmentDocumentHandler
{
    [AggregateHandler]
    public static async Task<(AssignmentDocumentAttached, Wolverine.Marten.Events)> Handle(
        AttachAssignmentDocument command,
        Assignment aggregate,
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

        var document = new AssignmentDocument(
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

        var @event = new AssignmentDocumentAttached(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            document,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
