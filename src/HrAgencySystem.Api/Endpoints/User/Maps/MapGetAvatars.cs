using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Port;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

/// <summary>
/// Who in the organization has a profile picture, and which file it is. The file id is what makes
/// the picture cacheable: the url never changes, so without it a replaced picture would keep
/// showing the old bytes.
/// </summary>
internal sealed record UserAvatarRef(Guid UserId, Guid AvatarFileId);

/// <summary>
/// Deliberately its own read rather than a field on <c>UserProjection</c>: that projection is built
/// from events, and a picture is not one - see the profile document for why. A list that wants to
/// draw faces asks this once instead of firing a request per row that mostly answers 404.
/// </summary>
internal static class MapGetAvatars
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/users/avatars
        group
            .MapGet(ApiEndpoints.Users.Avatars, Handler)
            .WithSummary("Get the profile pictures in the organization")
            .WithName("Get user avatars")
            .Produces<IReadOnlyList<UserAvatarRef>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IUserProfileRepository profiles,
        CancellationToken ct
    )
    {
        var stored = await profiles.GetManyAsync(user.GetOrganization, ct);

        return TypedResults.Ok(
            stored.Select(profile => new UserAvatarRef(profile.Id, profile.AvatarFileId)).ToList()
        );
    }
}
