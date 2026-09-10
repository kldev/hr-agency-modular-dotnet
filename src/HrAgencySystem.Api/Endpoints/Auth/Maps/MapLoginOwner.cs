using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Identity.Application.Owners.Login;
using HrAgencySystem.Identity.Application.Users.Login;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapLoginOwner
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/owner/login", Handler)
            .WithSummary("Login platform owner")
            .Produces<LoginUserResult>()
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(IMessageBus bus, LoginOwner command)
    {
        var result = await bus.InvokeAsync<LoginOwnerResult>(command);
        return TypedResults.Ok(result);
    }
}