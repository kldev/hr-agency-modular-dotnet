using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Projects.Application.Documents.Attach;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapAttachDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/projects/{projectId}/documents - multipart/form-data, one shot.
        endpoints
            .MapPost(ApiEndpoints.Projects.AttachDocument, Handler)
            .WithSummary("Attach a project document")
            .WithName("Attach project document")
            .DisableAntiforgery()
            .ProducesStandardErrors()
            .Produces<ProjectDocumentAttached>();
    }

    /// <summary>
    /// The one place in this codebase where an endpoint does more than translate HTTP into a
    /// command. A Wolverine command travels through the outbox and cannot carry a
    /// <see cref="Stream"/>, so the bytes are resolved into a <c>FileId</c> first - exactly as the
    /// token is resolved into an organization. Past this point no ASP.NET or storage type exists.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        IFormFile file,
        [FromForm] DocumentCategory category,
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
            new FileOwnerRef(FileOwnerKinds.Project, projectId),
            user.UserId,
            content,
            file.FileName,
            file.ContentType,
            ct
        );

        var result = await bus.InvokeAsync<ProjectDocumentAttached>(
            new AttachProjectDocument(
                projectId,
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
            $"/api/projects/{projectId}/documents/{result.Document.DocumentId}",
            result
        );
    }
}
