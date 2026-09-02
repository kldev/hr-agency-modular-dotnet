using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Owner.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/owners", Handler)
            .WithSummary("Create owner")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(IMessageBus bus, CreatePlatformOwner command)
    {
        var result = await bus.InvokeAsync<PlatformOwnerCreated>(command);
        
        return TypedResults.Created($"/api/owner/{result.PlatformOwnerId}", OwnerProjection.Create(result));
    }
}