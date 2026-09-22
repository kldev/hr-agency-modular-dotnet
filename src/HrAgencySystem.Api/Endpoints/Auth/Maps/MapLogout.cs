using System.ComponentModel;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Logout;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapLogout
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Auth.Logout, Handler)
            .WithSummary("End the session behind a refresh token")
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors()
            // Same reason as the refresh endpoint: signing out has to work with a dead access token,
            // and the refresh token being surrendered is what authenticates the call.
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, LogoutRequest request)
    {
        await bus.InvokeAsync(request.ToCommand());
        return TypedResults.NoContent();
    }

    internal record LogoutRequest(
        [property: Description(
            "The refresh token of the session to end. That session's token family is revoked; other sessions of the same user stay signed in."
        )]
            string RefreshToken
    )
    {
        public LogoutUser ToCommand() => new(RefreshToken);
    }
}
