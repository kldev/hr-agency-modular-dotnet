using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Create;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/teams
        endpoints
            .MapPost(ApiEndpoints.Teams.Create, Handler)
            .WithSummary("Create team")
            .WithName("Create team")
            .ProducesStandardErrors()
            .Produces<TeamCreated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        CreateTeamRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<TeamCreated>(
            request.ToCommand(organizationId: user.GetOrganization, createdBy: user.UserId),
            ct
        );

        return TypedResults.Created($"/api/teams/{result.TeamId}", result);
    }

    internal record CreateTeamRequest(string Name, IReadOnlyList<TeamMemberRequest> Members)
    {
        public CreateTeam ToCommand(OrganizationId organizationId, Guid createdBy)
        {
            return new CreateTeam(
                organizationId.Value,
                Name,
                [.. Members.Select(m => new CreateTeamMember(m.UserId, m.Role))],
                createdBy
            );
        }
    }

    internal record TeamMemberRequest(Guid UserId, TeamRole Role);
}
