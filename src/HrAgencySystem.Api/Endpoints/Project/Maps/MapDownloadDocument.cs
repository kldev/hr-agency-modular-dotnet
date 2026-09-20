using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapDownloadDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/projects/{projectId}/documents/{documentId}/content
        endpoints
            .MapGet(ApiEndpoints.Projects.DownloadDocument, Handler)
            .WithSummary("Download a project document")
            .WithName("Download project document")
            .ProducesStandardErrors();
    }

    /// <summary>
    /// Two checks, in this order: the API decides whether this person may see this project, and the
    /// file service decides whether the file belongs to this organization. The browser never learns
    /// a storage key, because there is none in the read model to learn.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] Guid documentId,
        IProjectsQueryRepository repository,
        IFileServiceClient files,
        CancellationToken ct
    )
    {
        var project =
            await repository.GetProject(user.GetOrganization, projectId, ct)
            ?? throw new NotFoundException("Project", projectId);

        var document =
            project.Documents.FirstOrDefault(d => d.DocumentId == documentId)
            ?? throw new NotFoundException("Project document", documentId);

        var content = await files.DownloadAsync(user.GetOrganization.Value, document.FileId, ct);

        if (content is null)
            throw new NotFoundException("Project document", documentId);

        return Results.File(content.Content, document.ContentType, document.FileName);
    }
}
