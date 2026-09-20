using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Documents.Attach;

public static class AttachProjectDocumentHandler
{
    [AggregateHandler]
    public static async Task<(ProjectDocumentAttached, Wolverine.Marten.Events)> Handle(
        AttachProjectDocument command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var (documentDate, validUntil, note) = ProjectDocumentFactory.Validate(
            command.DocumentDate,
            command.ValidUntil,
            command.Note,
            command.FileName
        );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectDocumentAttached(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            new ProjectDocument(
                Guid.NewGuid(),
                command.Category,
                command.FileId,
                command.FileName.Trim(),
                command.ContentType,
                command.Size,
                documentDate,
                validUntil,
                note
            ),
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
