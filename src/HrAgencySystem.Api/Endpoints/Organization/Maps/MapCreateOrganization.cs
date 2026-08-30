using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapCreateOrganization
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("", Handler)
            .WithSummary("Create organization")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(CreateOrganization command,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<OrganizationCreated>(command, ct);

        return TypedResults.Created(
            $"/api/organization/{result.OrganizationId}", result);
    }
}