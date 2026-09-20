using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Members.Add;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapAddMember
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/teams/{teamId}/members
        endpoints
            .MapPost(ApiEndpoints.Teams.AddMember, Handler)
            .WithSummary("Add a member to the team")
            .WithName("Add team member")
            .ProducesStandardErrors()
            .Produces<TeamMemberAdded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid teamId,
        AddTeamMemberRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<TeamMemberAdded>(
            request.ToCommand(
                teamId: teamId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal record AddTeamMemberRequest(Guid UserId, TeamRole Role)
    {
        public AddTeamMember ToCommand(Guid teamId, OrganizationId organizationId, Guid modifiedBy)
        {
            return new AddTeamMember(teamId, organizationId.Value, UserId, Role, modifiedBy);
        }
    }
}
