using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Users.Avatar.Remove;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapRemoveAvatar
{
    internal static void Map(RouteGroupBuilder group)
    {
        // DELETE /api/users/me/avatar
        group
            .MapDelete(ApiEndpoints.Users.Avatar, Handler)
            .WithSummary("Remove own profile picture")
            .WithName("Remove own avatar")
            .Produces<UserAvatarRemoved>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<UserAvatarRemoved> logger,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<UserAvatarRemoved>(
            new RemoveUserAvatar(user.UserId, user.GetOrganization, user.UserId),
            ct
        );

        // Record first, bytes after - and a storage that will not let go of them must not turn a
        // picture that is already gone from the profile into a failed request.
        try
        {
            await files.DeleteAsync(user.GetOrganization.Value, result.FileId, user.UserId, ct);
        }
        catch (FileServiceException exception)
        {
            logger.LogWarning(
                exception,
                "Removed avatar {FileId} of user {UserId} could not be deleted from storage",
                result.FileId,
                user.UserId
            );
        }

        return TypedResults.Ok(result);
    }
}
