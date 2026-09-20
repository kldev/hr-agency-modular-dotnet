using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Auth;
using HrAgencySystem.FileService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.FileService.Endpoints.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /files/{fileId}
        endpoints
            .MapGet(ServiceEndpoints.Get, Handler)
            .WithSummary("Get file metadata")
            .WithName("Get file metadata")
            .Produces<FileDescriptor>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        ServiceCaller caller,
        [FromRoute] Guid fileId,
        IFileStore store,
        CancellationToken ct
    )
    {
        var file = await store.GetAsync(caller.OrganizationId, fileId, ct);

        // Another organization's file is reported as absent, not as forbidden. Forbidden would
        // confirm that the id exists, which is the one thing a probing caller wants to learn.
        return file is null ? TypedResults.NotFound() : TypedResults.Ok(file);
    }
}
