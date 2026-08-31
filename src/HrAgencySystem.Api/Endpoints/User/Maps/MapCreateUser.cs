using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapCreateUser
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/users", Handler)
            .WithSummary("Create user")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);;
    }

    private static async Task<IResult> Handler(IMessageBus bus, CreateUser command)
    {
        var result = await bus.InvokeAsync<UserCreated>(command);
        
        return TypedResults.Created($"/api/users/{result.UserId}", UserProjection.Create(result));
    }
}