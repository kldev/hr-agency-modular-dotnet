using HrAgencySystem.Projects.Application.Documents.Attach;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Documents.UpdateMetadata;

public static class UpdateProjectDocumentMetadataHandler
{
    [AggregateHandler]
    public static async Task<(ProjectDocumentMetadataChanged, Wolverine.Marten.Events)> Handle(
        UpdateProjectDocumentMetadata command,
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

        var (documentDate, validUntil, note) = ProjectDocumentFactory.Validate(
            command.DocumentDate,
            command.ValidUntil,
            command.Note,
            document.FileName
        );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        // The file itself never changes here. Replacing a document means attaching a new one, so
        // that "what did we send them in March" keeps its answer.
        var @event = new ProjectDocumentMetadataChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            document with
            {
                Category = command.Category,
                DocumentDate = documentDate,
                ValidUntil = validUntil,
                Note = note,
            },
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
