using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Workers.Application.AssignmentDocuments.Remove;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapRemoveDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /api/assignments/{assignmentId}/documents/{documentId}
        endpoints
            .MapDelete(ApiEndpoints.Assignments.RemoveDocument, Handler)
            .WithSummary("Remove an assignment document")
            .WithName("Remove assignment document")
            .ProducesStandardErrors()
            .Produces<AssignmentDocumentRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        [FromRoute] Guid documentId,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<AssignmentDocumentRemoved> logger,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<AssignmentDocumentRemoved>(
            new RemoveAssignmentDocument(
                assignmentId,
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
                "Document {DocumentId} was removed from assignment {AssignmentId} but its file {FileId} could not be deleted.",
                documentId,
                assignmentId,
                result.FileId
            );
        }

        return TypedResults.Ok(result);
    }
}
