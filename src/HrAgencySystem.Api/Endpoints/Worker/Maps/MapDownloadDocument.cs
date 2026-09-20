using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Port;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapDownloadDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/workers/{workerId}/documents/{documentId}/content
        endpoints
            .MapGet(ApiEndpoints.Workers.DownloadDocument, Handler)
            .WithSummary("Download a worker document")
            .WithName("Download worker document")
            .ProducesStandardErrors();
    }

    /// <summary>
    /// Two checks, in this order: the API decides whether this person may see this file, and the
    /// file service decides whether the bytes belong to this organization. The browser never learns
    /// a storage key, because there is none in the read model to learn.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        [FromRoute] Guid documentId,
        IWorkersQueryRepository repository,
        IFileServiceClient files,
        CancellationToken ct
    )
    {
        var worker =
            await repository.GetWorker(user.GetOrganization, workerId, ct)
            ?? throw new NotFoundException("Worker", workerId);

        var document =
            worker.Documents.FirstOrDefault(d => d.DocumentId == documentId)
            ?? throw new NotFoundException("Worker document", documentId);

        var content = await files.DownloadAsync(user.GetOrganization.Value, document.FileId, ct);

        if (content is null)
            throw new NotFoundException("Worker document", documentId);

        return Results.File(content.Content, document.ContentType, document.FileName);
    }
}
