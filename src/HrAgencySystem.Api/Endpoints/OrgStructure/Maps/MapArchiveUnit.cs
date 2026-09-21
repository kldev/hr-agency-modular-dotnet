using HrAgencySystem.Agency.Application.OrgUnits.Archive;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapArchiveUnit
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.OrgStructure.ArchiveUnit, Handler)
            .WithSummary("Archive an organizational unit")
            .WithName("Archive org unit")
            .ProducesStandardErrors()
            .Produces<OrgUnitArchived>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitArchived>(
                new ArchiveOrgUnit(user.GetOrganization.Value, unitId, user.UserId),
                ct
            )
        );
}
