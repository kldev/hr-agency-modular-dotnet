using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Documents.Remove;

public static class RemoveProjectDocumentHandler
{
    public const string ReferencedByComplianceMessage =
        "This document is the proof recorded against a compliance requirement.";

    [AggregateHandler]
    public static async Task<(ProjectDocumentRemoved, Wolverine.Marten.Events)> Handle(
        RemoveProjectDocument command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var document =
            aggregate.DocumentById(command.DocumentId)
            ?? throw new NotFoundException("Project document", command.DocumentId);

        // Removing the proof while the tick stays would leave a requirement claiming evidence that
        // is no longer there - the one state this list must never be able to reach.
        if (aggregate.Compliance.Any(c => c.DocumentId == command.DocumentId))
            throw new BusinessRuleException(ReferencedByComplianceMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectDocumentRemoved(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            document.DocumentId,
            document.FileId,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
