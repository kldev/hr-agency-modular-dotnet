using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Avatar.Remove;
using HrAgencySystem.SharedKernel.Exception;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapRemoveAvatarFor
{
    internal static void Map(RouteGroupBuilder group)
    {
        // DELETE /api/users/{userId}/avatar
        group
            .MapDelete(ApiEndpoints.Users.AvatarFor, Handler)
            .WithSummary("Remove another member's profile picture")
            .WithName("Remove user avatar")
            .Produces<UserAvatarRemoved>()
            .ProducesStandardErrors()
            .RequireAuthorization(AdminPolicy.Name);
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated admin,
        IUserQueryRepository users,
        IFileServiceClient files,
        IMessageBus bus,
        ILogger<UserAvatarRemoved> logger,
        Guid userId,
        CancellationToken ct
    )
    {
        _ =
            await users.GetUser(admin.GetOrganization, userId, ct)
            ?? throw new NotFoundException("User", userId);

        var result = await bus.InvokeAsync<UserAvatarRemoved>(
            new RemoveUserAvatar(userId, admin.GetOrganization, admin.UserId),
            ct
        );

        // Record first, bytes after - and a storage that will not let go of them must not turn a
        // picture that is already gone from the profile into a failed request.
        try
        {
            await files.DeleteAsync(admin.GetOrganization.Value, result.FileId, admin.UserId, ct);
        }
        catch (FileServiceException exception)
        {
            logger.LogWarning(
                exception,
                "Removed avatar {FileId} of user {UserId} could not be deleted from storage",
                result.FileId,
                userId
            );
        }

        return TypedResults.Ok(result);
    }
}
