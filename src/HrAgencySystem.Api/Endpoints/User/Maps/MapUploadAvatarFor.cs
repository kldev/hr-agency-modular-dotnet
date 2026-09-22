using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Avatar.Change;
using HrAgencySystem.SharedKernel.Exception;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapUploadAvatarFor
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/users/{userId}/avatar - multipart/form-data, one shot.
        group
            .MapPost(ApiEndpoints.Users.AvatarFor, Handler)
            .WithSummary("Upload another member's profile picture")
            .WithName("Upload user avatar")
            .DisableAntiforgery()
            .Produces<UserAvatarChanged>()
            .ProducesStandardErrors()
            .RequireAuthorization(AdminPolicy.Name);
    }

    /// <summary>
    /// The administrative twin of uploading one's own picture. Two things differ and both matter:
    /// the file is owned by the person in the url while the administrator is recorded as the one who
    /// put it there, and the target is looked up first - an id from another organization or one that
    /// does not exist is a 404 before any bytes travel to the file service.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated admin,
        IFormFile file,
        IUserQueryRepository users,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<UserAvatarChanged> logger,
        Guid userId,
        CancellationToken ct
    )
    {
        _ =
            await users.GetUser(admin.GetOrganization, userId, ct)
            ?? throw new NotFoundException("User", userId);

        AvatarUploadPolicy.Validate(file.ContentType, file.Length);

        await using var content = file.OpenReadStream();

        var stored = await files.UploadAsync(
            admin.GetOrganization.Value,
            new FileOwnerRef(FileOwnerKinds.User, userId),
            admin.UserId,
            content,
            file.FileName,
            file.ContentType,
            ct
        );

        var result = await bus.InvokeAsync<UserAvatarChanged>(
            new ChangeUserAvatar(
                userId,
                admin.GetOrganization,
                stored.FileId,
                stored.FileName,
                stored.ContentType,
                stored.Size,
                admin.UserId
            ),
            ct
        );

        await DeletePrevious(result, admin, files, logger, ct);

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Record first, bytes after - a storage that refuses the delete must not fail a picture that
    /// was already replaced. The same order the self-service upload keeps.
    /// </summary>
    private static async Task DeletePrevious(
        UserAvatarChanged result,
        AppUserAuthenticated admin,
        IFileServiceClient files,
        ILogger logger,
        CancellationToken ct
    )
    {
        if (result.PreviousFileId is not { } previous)
            return;

        try
        {
            await files.DeleteAsync(admin.GetOrganization.Value, previous, admin.UserId, ct);
        }
        catch (FileServiceException exception)
        {
            logger.LogWarning(
                exception,
                "Replaced avatar {FileId} of user {UserId} could not be deleted from storage",
                previous,
                result.UserId
            );
        }
    }
}
