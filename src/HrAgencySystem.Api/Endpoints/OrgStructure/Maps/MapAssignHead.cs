using System.ComponentModel;
using HrAgencySystem.Agency.Application.OrgUnits.AssignHead;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapAssignHead
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPut(ApiEndpoints.OrgStructure.Head, Handler)
            .WithSummary("Name the head of a unit")
            .WithName("Assign org unit head")
            .ProducesStandardErrors()
            .Produces<OrgUnitHeadAssigned>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        AssignOrgUnitHeadRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitHeadAssigned>(
                request.ToCommand(unitId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record AssignOrgUnitHeadRequest(
        [property: Description(
            "The person who heads the unit - already one of its members. They become the supervisor of its people and of units below without a head of their own."
        )]
            Guid HeadUserId
    )
    {
        public AssignOrgUnitHead ToCommand(
            Guid unitId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(organizationId.Value, unitId, HeadUserId, modifiedBy);
    }
}
