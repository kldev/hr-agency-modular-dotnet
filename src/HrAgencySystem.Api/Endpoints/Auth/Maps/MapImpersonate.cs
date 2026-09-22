using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Impersonate;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapImpersonate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/auth/impersonate/{userId}
        group
            .MapPost(ApiEndpoints.Auth.Impersonate, Handler)
            .WithSummary("Sign in as another member of the organization, without their password")
            .WithName("Impersonate user")
            .Produces<ImpersonationResult>()
            .ProducesStandardErrors()
            .RequireAuthorization(AdminPolicy.Name);
    }

    /// <summary>
    /// Filed with the other endpoints that mint a token rather than with the ones that manage users:
    /// this changes nothing about the person in the url, it hands out a bearer - which is what every
    /// route under <c>/api/auth</c> has in common.
    /// <para>
    /// The organization comes from the caller's own token and is never a parameter, so there is no
    /// id to swap for somebody else's tenant.
    /// </para>
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated admin,
        IMessageBus bus,
        Guid userId,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ImpersonationResult>(
            new ImpersonateUser(userId, admin.GetOrganization, admin.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
