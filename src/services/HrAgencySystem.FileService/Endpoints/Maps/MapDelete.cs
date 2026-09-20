using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.FileService.Endpoints.Maps;

internal static class MapDelete
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /files/{fileId}
        endpoints
            .MapDelete(ServiceEndpoints.Delete, Handler)
            .WithSummary("Delete file")
            .WithName("Delete file")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        ServiceCaller caller,
        [FromRoute] Guid fileId,
        IFileStore store,
        CancellationToken ct
    )
    {
        var deleted = await store.DeleteAsync(caller.OrganizationId, fileId, caller.ActorId, ct);

        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
