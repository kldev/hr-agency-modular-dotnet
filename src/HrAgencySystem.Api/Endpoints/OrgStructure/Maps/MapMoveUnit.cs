using System.ComponentModel;
using HrAgencySystem.Agency.Application.OrgUnits.Move;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapMoveUnit
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT .../parent - a reorganisation is a unit changing what it hangs under, and everyone
        // inside it gets a new supervisor without anybody editing a person.
        endpoints
            .MapPut(ApiEndpoints.OrgStructure.MoveUnit, Handler)
            .WithSummary("Move an organizational unit under another")
            .WithName("Move org unit")
            .ProducesStandardErrors()
            .Produces<OrgUnitMoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        MoveOrgUnitRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitMoved>(
                request.ToCommand(unitId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record MoveOrgUnitRequest(
        [property: Description(
            "The unit to hang this one under. Not the unit itself and not one of its own units; the top unit cannot move."
        )]
            Guid ParentId
    )
    {
        public MoveOrgUnit ToCommand(Guid unitId, OrganizationId organizationId, Guid modifiedBy) =>
            new(organizationId.Value, unitId, ParentId, modifiedBy);
    }
}
