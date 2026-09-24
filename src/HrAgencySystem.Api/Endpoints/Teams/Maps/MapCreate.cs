using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Create;
using HrAgencySystem.Teams.Contracts;
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

    internal record CreateTeamRequest(
        [property: Description("The team's name.")] string Name,
        [property: Description(
            "Its first members, each with a seat - at least one, and nobody twice."
        )]
            IReadOnlyList<TeamMemberRequest> Members
    )
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

    internal record TeamMemberRequest(
        [property: Description("The person joining.")] Guid UserId,
        [property: Description("Their seat in the team: Sales, Recruiter, Operations or Lead.")]
            TeamRole Role
    );
}
