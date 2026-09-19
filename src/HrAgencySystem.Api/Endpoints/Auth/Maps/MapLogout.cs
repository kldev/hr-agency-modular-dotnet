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

    internal record LogoutRequest(string RefreshToken)
    {
        public LogoutUser ToCommand() => new(RefreshToken);
    }
}
