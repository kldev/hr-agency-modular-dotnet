using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Workers.Application.AssignmentDocuments.Attach;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapAttachDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/assignments/{assignmentId}/documents - the A1, the host country filing, the
        // contract for this posting. A passport goes on the person instead.
        endpoints
            .MapPost(ApiEndpoints.Assignments.AttachDocument, Handler)
            .WithSummary("Attach an assignment document")
            .WithName("Attach assignment document")
            .DisableAntiforgery()
            .ProducesStandardErrors()
            .Produces<AssignmentDocumentAttached>();
    }

    /// <summary>
    /// The sanctioned exception to "endpoints only translate HTTP into a command": a Wolverine
    /// command travels through the outbox and cannot carry a <see cref="Stream"/>, so the bytes are
    /// resolved into a <c>FileId</c> here, before the command exists.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        IFormFile file,
        [FromForm] AssignmentDocumentCategory category,
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
            new FileOwnerRef(FileOwnerKinds.Assignment, assignmentId),
            user.UserId,
            content,
            file.FileName,
            file.ContentType,
            ct
        );

        var result = await bus.InvokeAsync<AssignmentDocumentAttached>(
            new AttachAssignmentDocument(
                assignmentId,
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
            $"/api/assignments/{assignmentId}/documents/{result.Document.DocumentId}",
            result
        );
    }
}
