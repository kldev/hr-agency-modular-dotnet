using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Teams.Application.Members.Remove;
using HrAgencySystem.Teams.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapRemoveMember
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /api/teams/{teamId}/members/{userId}
        endpoints
            .MapDelete(ApiEndpoints.Teams.RemoveMember, Handler)
            .WithSummary("Remove a member from the team")
            .WithName("Remove team member")
            .ProducesStandardErrors()
            .Produces<TeamMemberRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid teamId,
        [FromRoute] Guid userId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<TeamMemberRemoved>(
            new RemoveTeamMember(
                TeamId: teamId,
                OrganizationId: user.OrganizationId,
                UserId: userId,
                ModifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
