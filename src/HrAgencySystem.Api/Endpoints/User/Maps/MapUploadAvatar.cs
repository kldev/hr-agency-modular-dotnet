using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Users.Avatar.Change;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapUploadAvatar
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/users/me/avatar - multipart/form-data, one shot.
        group
            .MapPost(ApiEndpoints.Users.Avatar, Handler)
            .WithSummary("Upload own profile picture")
            .WithName("Upload own avatar")
            .DisableAntiforgery()
            .Produces<UserAvatarChanged>()
            .ProducesStandardErrors();
    }

    /// <summary>
    /// Like the project document upload, this endpoint does more than translate HTTP into a command:
    /// a command goes through the outbox and cannot carry a <see cref="Stream"/>, so the bytes become
    /// a <c>FileId</c> first. The size and type are checked before that, so a picture that was never
    /// going to be accepted does not first travel to the file service.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormFile file,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<UserAvatarChanged> logger,
        CancellationToken ct
    )
    {
        AvatarUploadPolicy.Validate(file.ContentType, file.Length);

        await using var content = file.OpenReadStream();

        var stored = await files.UploadAsync(
            user.GetOrganization.Value,
            new FileOwnerRef(FileOwnerKinds.User, user.UserId),
            user.UserId,
            content,
            file.FileName,
            file.ContentType,
            ct
        );

        var result = await bus.InvokeAsync<UserAvatarChanged>(
            new ChangeUserAvatar(
                user.UserId,
                user.GetOrganization,
                stored.FileId,
                stored.FileName,
                stored.ContentType,
                stored.Size
            ),
            ct
        );

        await DeletePrevious(result, user, files, logger, ct);

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// The record goes first and the bytes after it, the same order the project documents keep:
    /// bytes nobody can reach are untidy, a row pointing at a deleted file is a lie. A storage that
    /// refuses the delete therefore must not fail a picture that was already replaced.
    /// </summary>
    private static async Task DeletePrevious(
        UserAvatarChanged result,
        AppUserAuthenticated user,
        IFileServiceClient files,
        ILogger logger,
        CancellationToken ct
    )
    {
        if (result.PreviousFileId is not { } previous)
            return;

        try
        {
            await files.DeleteAsync(user.GetOrganization.Value, previous, user.UserId, ct);
        }
        catch (FileServiceException exception)
        {
            logger.LogWarning(
                exception,
                "Replaced avatar {FileId} of user {UserId} could not be deleted from storage",
                previous,
                user.UserId
            );
        }
    }
}
