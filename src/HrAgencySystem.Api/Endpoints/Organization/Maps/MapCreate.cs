using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapCreate
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/organization", Handler)
            .WithSummary("Create organization")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(OwnerAuthenticated owner, CreateOrganizationRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<OrganizationCreated>(request.ToCommand(owner.Id), ct);

        return TypedResults.Created(
            $"/api/organization/{result.OrganizationId}", result);
    }
}

internal sealed record CreateOrganizationRequest(string Name, string Slug)
{
    public CreateOrganization ToCommand(Guid createdBy) => new (Name, Slug, createdBy);
}