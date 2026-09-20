using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Workers.Application.WorkerDocuments.Attach;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapAttachDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/workers/{workerId}/documents
        endpoints
            .MapPost(ApiEndpoints.Workers.AttachDocument, Handler)
            .WithSummary("Attach a worker document")
            .WithName("Attach worker document")
            .DisableAntiforgery()
            .ProducesStandardErrors()
            .Produces<WorkerDocumentAttached>();
    }

    /// <summary>
    /// The sanctioned exception to "endpoints only translate HTTP into a command": a Wolverine
    /// command travels through the outbox and cannot carry a <see cref="Stream"/>, so the bytes are
    /// resolved into a <c>FileId</c> here, before the command exists.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        IFormFile file,
        [FromForm] WorkerDocumentCategory category,
        [FromForm] DateOnly documentDate,
        [FromForm] DateOnly? validUntil,
        [FromForm] string? note,
        IFileServiceClient files,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        await using var content = file.OpenReadStream();

        var stored = await files.UploadAsync(
            user.GetOrganization.Value,
            new FileOwnerRef(FileOwnerKinds.Worker, workerId),
            user.UserId,
            content,
            file.FileName,
            file.ContentType,
            ct
        );

        var result = await bus.InvokeAsync<WorkerDocumentAttached>(
            new AttachWorkerDocument(
                workerId,
                user.GetOrganization.Value,
                category,
                stored.FileId,
                stored.FileName,
                stored.ContentType,
                stored.Size,
                documentDate,
                validUntil,
                note,
                user.UserId
            ),
            ct
        );

        return TypedResults.Created(
            $"/api/workers/{workerId}/documents/{result.Document.DocumentId}",
            result
        );
    }
}
