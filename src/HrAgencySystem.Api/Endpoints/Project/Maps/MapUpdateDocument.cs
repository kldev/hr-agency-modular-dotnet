using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Documents.UpdateMetadata;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapUpdateDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/documents/{documentId}
        endpoints
            .MapPut(ApiEndpoints.Projects.UpdateDocument, Handler)
            .WithSummary("Update project document metadata")
            .WithName("Update project document metadata")
            .ProducesStandardErrors()
            .Produces<ProjectDocumentMetadataChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] Guid documentId,
        UpdateProjectDocumentRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectDocumentMetadataChanged>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                documentId: documentId,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Metadata only. Replacing the file means attaching a new document, so that what was sent in
    /// March keeps its answer.
    /// </summary>
    internal sealed record UpdateProjectDocumentRequest(
        DocumentCategory Category,
        DateOnly DocumentDate,
        DateOnly? ValidUntil,
        string? Note
    )
    {
        public UpdateProjectDocumentMetadata ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            Guid documentId,
            Guid modifiedBy
        ) =>
            new(
                projectId,
                organizationId.Value,
                documentId,
                Category,
                DocumentDate,
                ValidUntil,
                Note,
                modifiedBy
            );
    }
}
