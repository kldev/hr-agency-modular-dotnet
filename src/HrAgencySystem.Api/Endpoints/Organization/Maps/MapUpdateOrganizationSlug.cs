using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

public static class MapUpdateOrganizationSlug
{
    internal record UpdateSlug(string Slug);
    
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{organizationId}/slug",
            async (
                IMessageBus bus,
                Guid organizationId,
                [FromBody] UpdateSlug request,
                CancellationToken ct) =>
            {
                var command = new UpdateOrganizationSlug(request.Slug, organizationId);
                var result = await bus.InvokeAsync<OrganizationSlugUpdated>(command, ct);

                return TypedResults.Ok(result);
            });
    }
}