using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Workers.Application.WorkerDocuments.Remove;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapRemoveDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /api/workers/{workerId}/documents/{documentId}
        endpoints
            .MapDelete(ApiEndpoints.Workers.RemoveDocument, Handler)
            .WithSummary("Remove a worker document")
            .WithName("Remove worker document")
            .ProducesStandardErrors()
            .Produces<WorkerDocumentRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        [FromRoute] Guid documentId,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<WorkerDocumentRemoved> logger,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkerDocumentRemoved>(
            new RemoveWorkerDocument(workerId, user.GetOrganization.Value, documentId, user.UserId),
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
                "Document {DocumentId} was removed from worker {WorkerId} but its file {FileId} could not be deleted.",
                documentId,
                workerId,
                result.FileId
            );
        }

        return TypedResults.Ok(result);
    }
}
