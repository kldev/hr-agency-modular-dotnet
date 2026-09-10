using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Identity.Application.Users.Login;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapLoginUser
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/auth/login", Handler)
            .WithSummary("Login organization user")
            .Produces<LoginUserResult>()
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, LoginUser command)
    {
        var result = await bus.InvokeAsync<LoginUserResult>(command);
        return TypedResults.Ok(result);
    }
}