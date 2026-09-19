using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Rename;
using HrAgencySystem.Teams.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapRename
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/teams/{teamId}/name
        endpoints
            .MapPut(ApiEndpoints.Teams.Rename, Handler)
            .WithSummary("Rename team")
            .WithName("Rename team")
            .ProducesStandardErrors()
            .Produces<TeamRenamed>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid teamId,
        RenameTeamRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<TeamRenamed>(
            request.ToCommand(
                teamId: teamId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal record RenameTeamRequest(string Name)
    {
        public RenameTeam ToCommand(Guid teamId, OrganizationId organizationId, Guid modifiedBy)
        {
            return new RenameTeam(teamId, organizationId.Value, Name, modifiedBy);
        }
    }
}
