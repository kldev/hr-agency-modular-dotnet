using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Projects.Application.Documents.Remove;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapRemoveDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /api/projects/{projectId}/documents/{documentId}
        endpoints
            .MapDelete(ApiEndpoints.Projects.RemoveDocument, Handler)
            .WithSummary("Remove a project document")
            .WithName("Remove project document")
            .ProducesStandardErrors()
            .Produces<ProjectDocumentRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] Guid documentId,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<ProjectDocumentRemoved> logger,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectDocumentRemoved>(
            new RemoveProjectDocument(
                projectId,
                user.GetOrganization.Value,
                documentId,
                user.UserId
            ),
            ct
        );

        // The record goes first. Bytes nobody can reach are untidy; a live record over deleted bytes
        // is a lie, and this order can only ever produce the former.
        try
        {
            await files.DeleteAsync(user.GetOrganization.Value, result.FileId, user.UserId, ct);
        }
        catch (FileServiceException exception)
        {
            logger.LogError(
                exception,
                "Document {DocumentId} was removed from project {ProjectId} but its file {FileId} could not be deleted.",
                documentId,
                projectId,
                result.FileId
            );
        }

        return TypedResults.Ok(result);
    }
}
