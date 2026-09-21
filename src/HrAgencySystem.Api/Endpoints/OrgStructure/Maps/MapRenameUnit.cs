using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapRenameUnit
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPut(ApiEndpoints.OrgStructure.RenameUnit, Handler)
            .WithSummary("Rename an organizational unit")
            .WithName("Rename org unit")
            .ProducesStandardErrors()
            .Produces<OrgUnitRenamed>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        RenameOrgUnitRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitRenamed>(
                request.ToCommand(unitId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record RenameOrgUnitRequest(string Name)
    {
        public RenameOrgUnit ToCommand(
            Guid unitId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(organizationId.Value, unitId, Name, modifiedBy);
    }
}
