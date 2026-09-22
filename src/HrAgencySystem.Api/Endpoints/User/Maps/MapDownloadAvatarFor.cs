using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapDownloadAvatarFor
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/users/{userId}/avatar
        group
            .MapGet(ApiEndpoints.Users.AvatarFor, Handler)
            .WithSummary("Download another member's profile picture")
            .WithName("Download user avatar")
            .ProducesStandardErrors();
    }

    /// <summary>
    /// Open to every authenticated member of the organization, unlike setting the picture: faces
    /// are drawn next to people in the user list and in the org chart, so a colleague has to be
    /// able to read one. Somebody else's tenant answers 404 because the lookup is scoped to the
    /// caller's organization - there is no id to try.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IUserProfileRepository profiles,
        IFileServiceClient files,
        Guid userId,
        CancellationToken ct
    )
    {
        var profile =
            await profiles.GetAsync(user.GetOrganization, UserId.From(userId), ct)
            ?? throw new NotFoundException("Avatar", userId);

        var content = await files.DownloadAsync(
            user.GetOrganization.Value,
            profile.AvatarFileId,
            ct
        );

        if (content is null)
            throw new NotFoundException("Avatar", userId);

        return Results.File(content.Content, profile.AvatarContentType);
    }
}
