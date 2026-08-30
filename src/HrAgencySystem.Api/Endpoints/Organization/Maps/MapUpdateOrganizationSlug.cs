using HrAgencySystem.Organization.Application.Commands;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

public static class MapUpdateOrganizationSlug
{
    internal record UpdateSlug(string Slug);

    internal record UpdateSlugResponse(string Slug);

    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{organizationId}/slug",
            async (
                IMessageBus bus,
                Guid organizationId,
                IDocumentSession session,
                [FromBody] UpdateSlug request,
                CancellationToken ct) =>
            {
                var command = new UpdateOrganizationSlug(request.Slug, organizationId); 
                var result = await bus.InvokeAsync<HrAgencySystem.Organization.Domain.Organization>(command, ct);
                
                return TypedResults.Ok(new UpdateSlugResponse(result!.Slug.Value));
            });
    }
}