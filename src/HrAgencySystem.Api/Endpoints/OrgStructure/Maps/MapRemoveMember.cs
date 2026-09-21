using HrAgencySystem.Agency.Application.OrgUnits.RemoveMember;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapRemoveMember
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapDelete(ApiEndpoints.OrgStructure.Member, Handler)
            .WithSummary("Take somebody out of a unit")
            .WithName("Remove org unit member")
            .ProducesStandardErrors()
            .Produces<OrgUnitMemberRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        [FromRoute] Guid userId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitMemberRemoved>(
                new RemoveOrgUnitMember(user.GetOrganization.Value, unitId, userId, user.UserId),
                ct
            )
        );
}
