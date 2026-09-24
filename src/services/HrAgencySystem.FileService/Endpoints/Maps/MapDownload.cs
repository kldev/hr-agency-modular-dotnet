using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.FileService.Endpoints.Maps;

internal static class MapDownload
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /files/{fileId}/content
        endpoints
            .MapGet(ServiceEndpoints.Content, Handler)
            .WithSummary("Download file")
            .WithName("Download file")
            .Produces<IResult>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        ServiceCaller caller,
        [FromRoute] Guid fileId,
        IFileStore store,
        CancellationToken ct
    )
    {
        var content = await store.OpenReadAsync(caller.OrganizationId, fileId, ct);

        if (content is null)
            return TypedResults.NotFound();

        return Results.File(content.Content, content.ContentType, content.FileName);
    }
}
