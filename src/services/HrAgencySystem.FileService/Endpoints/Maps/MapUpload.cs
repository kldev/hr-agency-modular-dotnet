using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Auth;
using HrAgencySystem.FileService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.FileService.Endpoints.Maps;

internal static class MapUpload
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /files
        endpoints
            .MapPost(ServiceEndpoints.Upload, Handler)
            .WithSummary("Upload file")
            .WithName("Upload file")
            .DisableAntiforgery()
            .Produces<FileDescriptor>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(
        ServiceCaller caller,
        IFormFile file,
        [FromForm] string ownerKind,
        [FromForm] Guid ownerId,
        IFileStore store,
        CancellationToken ct
    )
    {
        await using var content = file.OpenReadStream();

        var result = await store.StoreAsync(
            caller.OrganizationId,
            new FileOwnerRef(ownerKind, ownerId),
            caller.ActorId,
            content,
            file.Length,
            file.FileName,
            file.ContentType,
            ct
        );

        if (result.Rejection is not null)
            return TypedResults.BadRequest(new UploadRejected(result.Rejection));

        return TypedResults.Created($"/files/{result.File!.FileId}", result.File);
    }

    internal sealed record UploadRejected(string Reason);
}
