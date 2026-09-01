using HrAgencySystem.Identity.Application.Commands;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapLoginUser
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/auth/login", Handler)
            .WithSummary("Login organization user").AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, LoginUser command)
    {
        var result = await bus.InvokeAsync<LoginUserResult>(command);
        return TypedResults.Ok(result);
    }
}