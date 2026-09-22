using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Members.ChangeRole;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapChangeMemberRole
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/teams/{teamId}/members/{userId}/role
        endpoints
            .MapPut(ApiEndpoints.Teams.ChangeMemberRole, Handler)
            .WithSummary("Change a team member's role")
            .WithName("Change team member role")
            .ProducesStandardErrors()
            .Produces<TeamMemberRoleChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid teamId,
        [FromRoute] Guid userId,
        ChangeTeamMemberRoleRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<TeamMemberRoleChanged>(
            request.ToCommand(
                teamId: teamId,
                organizationId: user.GetOrganization,
                userId: userId,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal record ChangeTeamMemberRoleRequest(
        [property: Description("The member's new seat: Sales, Recruiter, Operations or Lead.")]
            TeamRole Role
    )
    {
        public ChangeTeamMemberRole ToCommand(
            Guid teamId,
            OrganizationId organizationId,
            Guid userId,
            Guid modifiedBy
        )
        {
            return new ChangeTeamMemberRole(teamId, organizationId.Value, userId, Role, modifiedBy);
        }
    }
}
