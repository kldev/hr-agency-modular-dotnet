using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Organization.Application.Update;
using HrAgencySystem.Organization.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

public static class MapUpdate
{
    public static void Map(RouteGroupBuilder group)
    {
        // PUT /api/organization/{organizationId}
        group.MapPut("{organizationId:guid}", Handler)
            .WithSummary("Update organization data")
            .WithName("Update organization data")
            .Produces<OrganizationUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        OwnerAuthenticated owner,
        Guid organizationId,
        [FromBody]OrganizationRequest request,
        CancellationToken ct)
    {
        var command = new UpdateOrganization(organizationId, request.Name, request.Slug, owner.Id, request.EmailDomains);
        var result = await bus.InvokeAsync<OrganizationUpdated>(command, ct);

        return TypedResults.Ok(result);
    }
}