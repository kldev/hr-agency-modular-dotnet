using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

public static class MapUpdateOrganizationSlug
{
    internal record UpdateSlug(string Slug);

    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{organizationId}/slug", Handler)
            .WithSummary("Update organization slug")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        Guid organizationId,
        IDocumentSession session,
        [FromBody] UpdateSlug request,
        CancellationToken ct)
    {
        var command = new UpdateOrganizationSlug(request.Slug, organizationId);
        var result = await bus.InvokeAsync<OrganizationSlugUpdated>(command, ct);

        return TypedResults.Ok(result);
    }
}