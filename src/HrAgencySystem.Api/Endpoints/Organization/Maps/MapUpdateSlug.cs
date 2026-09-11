using HrAgencySystem.Api.Common;
using HrAgencySystem.Organization.Application.UpdateSlug;
using HrAgencySystem.Organization.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

public static class MapUpdateSlug
{
    internal record UpdateSlug(string Slug);

    public static void Map(RouteGroupBuilder group)
    {
        // PUT /api/organization/{organizationId}/slug
        group.MapPut("{organizationId}/slug", Handler)
            .WithSummary("Update organization slug")
            .Produces<OrganizationSlugUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        Guid organizationId,
        [FromBody] UpdateSlug request,
        CancellationToken ct)
    {
        var command = new UpdateOrganizationSlug(request.Slug, organizationId);
        var result = await bus.InvokeAsync<OrganizationSlugUpdated>(command, ct);

        return TypedResults.Ok(result);
    }
}