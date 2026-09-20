using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.WorkerDocuments;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.AssignmentDocuments.UpdateMetadata;

public static class UpdateAssignmentDocumentMetadataHandler
{
    public const string UnknownDocumentMessage = "That document is not on this assignment.";

    [AggregateHandler]
    public static async Task<(AssignmentDocumentMetadataChanged, Wolverine.Marten.Events)> Handle(
        UpdateAssignmentDocumentMetadata command,
        Domain.Assignment aggregate,
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

        var changed = document with
        {
            Category = command.Category,
            DocumentDate = command.DocumentDate,
            ValidUntil = command.ValidUntil,
            Note = note,
        };

        var @event = new AssignmentDocumentMetadataChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            changed,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
