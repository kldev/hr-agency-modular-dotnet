using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Owners.Login;
using HrAgencySystem.Identity.Application.Users.Login;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapLoginOwner
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/owner/login", Handler)
            .WithSummary("Login platform owner")
            .WithName("Login platform owner")
            .Produces<LoginUserResult>()
            .ProducesStandardErrors()
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, LoginOwner command)
    {
        var result = await bus.InvokeAsync<LoginOwnerResult>(command);
        return TypedResults.Ok(result);
    }
}