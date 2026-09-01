using HrAgencySystem.Identity.Application.Commands;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapLoginOwner
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/owner/login", Handler)
            .WithSummary("Login platform owner").AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, LoginOwner command)
    {
        var result = await bus.InvokeAsync<LoginOwnerResult>(command);
        return TypedResults.Ok(result);
    }
}