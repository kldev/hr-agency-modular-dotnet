using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Port;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapDownloadDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/assignments/{assignmentId}/documents/{documentId}/content
        endpoints
            .MapGet(ApiEndpoints.Assignments.DownloadDocument, Handler)
            .WithSummary("Download an assignment document")
            .WithName("Download assignment document")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        [FromRoute] Guid documentId,
        IAssignmentsQueryRepository repository,
        IFileServiceClient files,
        CancellationToken ct
    )
    {
        var assignment =
            await repository.GetAssignment(user.GetOrganization, assignmentId, ct)
            ?? throw new NotFoundException("Assignment", assignmentId);

        var document =
            assignment.Documents.FirstOrDefault(d => d.DocumentId == documentId)
            ?? throw new NotFoundException("Assignment document", documentId);

        var content = await files.DownloadAsync(user.GetOrganization.Value, document.FileId, ct);

        if (content is null)
            throw new NotFoundException("Assignment document", documentId);

        return Results.File(content.Content, document.ContentType, document.FileName);
    }
}
