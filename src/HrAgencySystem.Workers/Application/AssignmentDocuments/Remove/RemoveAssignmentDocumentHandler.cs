using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.AssignmentDocuments.Remove;

public static class RemoveAssignmentDocumentHandler
{
    public const string UnknownDocumentMessage = "That document is not on this assignment.";

    public const string ReferencedByComplianceMessage =
        "This document is the proof behind a recorded requirement. Record the requirement without it first.";

    [AggregateHandler]
    public static async Task<(AssignmentDocumentRemoved, Wolverine.Marten.Events)> Handle(
        RemoveAssignmentDocument command,
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

        // Removing the proof while the tick stays would leave a requirement claiming evidence that
        // is no longer there - the one state this register must never be able to reach.
        if (aggregate.Compliance.Any(c => c.DocumentId == command.DocumentId))
            throw new BusinessRuleException(ReferencedByComplianceMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new AssignmentDocumentRemoved(
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
