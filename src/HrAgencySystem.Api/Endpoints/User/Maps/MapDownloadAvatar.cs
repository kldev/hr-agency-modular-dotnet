using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapDownloadAvatar
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/users/me/avatar
        group
            .MapGet(ApiEndpoints.Users.Avatar, Handler)
            .WithSummary("Download own profile picture")
            .WithName("Download own avatar")
            .ProducesStandardErrors();
    }

    /// <summary>
    /// Stays behind the ordinary authentication, although the browser reaches it from an
    /// <c>&lt;img&gt;</c> tag: the page talks to a same-origin proxy that attaches the bearer token,
    /// so the tag needs no credential of its own and no presigned url has to exist.
    /// <para>
    /// Served without a download name, unlike a document - this is meant to be rendered, not saved.
    /// </para>
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IUserProfileRepository profiles,
        IFileServiceClient files,
        CancellationToken ct
    )
    {
        var profile =
            await profiles.GetAsync(user.GetOrganization, UserId.From(user.UserId), ct)
            ?? throw new NotFoundException("Avatar", user.UserId);

        var content = await files.DownloadAsync(
            user.GetOrganization.Value,
            profile.AvatarFileId,
            ct
        );

        if (content is null)
            throw new NotFoundException("Avatar", user.UserId);

        return Results.File(content.Content, profile.AvatarContentType);
    }
}
