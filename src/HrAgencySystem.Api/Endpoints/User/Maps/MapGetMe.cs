using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

/// <summary>
/// The profile as a person sees it about themselves. Deliberately not the same thing as
/// <c>GET /api/user/me</c>: that one answers "is this session valid and what may it do" from the
/// token alone, and is bound on every endpoint in the application. This one is the display shape,
/// and it is the only place the two sources - the user read model and the profile document - are
/// put together.
/// </summary>
internal sealed record MyProfileResponse(UserProjection User, Guid? AvatarFileId);

internal static class MapGetMe
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Users.Me, Handler)
            .WithSummary("Get own profile")
            .WithName("Get own profile")
            .Produces<MyProfileResponse>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IDocumentSession session,
        IUserProfileRepository profiles,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<UserProjection>()
            .Where(z => z.Id == user.UserId && z.OrganizationId == user.OrganizationId)
            .SingleOrDefaultAsync(ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("User", user.UserId));
        }

        var profile = await profiles.GetAsync(user.GetOrganization, UserId.From(user.UserId), ct);

        return TypedResults.Ok(new MyProfileResponse(result, profile?.AvatarFileId));
    }
}
