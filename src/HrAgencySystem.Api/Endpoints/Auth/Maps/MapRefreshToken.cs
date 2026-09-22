using System.ComponentModel;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Login;
using HrAgencySystem.Identity.Application.Users.Refresh;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapRefreshToken
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Auth.Refresh, Handler)
            .WithSummary("Exchange a refresh token for a new token pair")
            .WithName("Refresh access token")
            .Produces<LoginUserResult>()
            .ProducesStandardErrors()
            // Anonymous on purpose: the access token is expected to be expired by the time a client
            // gets here, and the refresh token is the credential being presented.
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, RefreshTokenRequest request)
    {
        var result = await bus.InvokeAsync<LoginUserResult>(request.ToCommand());
        return TypedResults.Ok(result);
    }

    internal record RefreshTokenRequest(
        [property: Description(
            "The refresh token received at sign-in or at the last refresh. It is spent by this call and replaced by a new one; presenting a spent token again revokes the whole session."
        )]
            string RefreshToken
    )
    {
        public RefreshAccessToken ToCommand() => new(RefreshToken);
    }
}
