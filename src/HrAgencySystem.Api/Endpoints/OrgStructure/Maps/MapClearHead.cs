using HrAgencySystem.Agency.Application.OrgUnits.ClearHead;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapClearHead
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE .../head - leaves the unit to whoever heads the one above it, which is a normal
        // shape rather than a hole to fill.
        endpoints
            .MapDelete(ApiEndpoints.OrgStructure.Head, Handler)
            .WithSummary("Leave a unit without its own head")
            .WithName("Clear org unit head")
            .ProducesStandardErrors()
            .Produces<OrgUnitHeadCleared>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitHeadCleared>(
                new ClearOrgUnitHead(user.GetOrganization.Value, unitId, user.UserId),
                ct
            )
        );
}
