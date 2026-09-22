using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.AssignTeam;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapAssignTeam
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/team
        endpoints
            .MapPut(ApiEndpoints.Projects.AssignTeam, Handler)
            .WithSummary("Assign project team")
            .WithName("Assign project team")
            .ProducesStandardErrors()
            .Produces<ProjectTeamAssigned>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        AssignProjectTeamRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectTeamAssigned>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record AssignProjectTeamRequest(
        [property: Description("The recruitment team that staffs the project.")] Guid TeamId
    )
    {
        public AssignProjectTeam ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(projectId, organizationId.Value, TeamId, modifiedBy);
    }
}
