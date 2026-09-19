using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Teams.Application.Suggestion;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapTeam
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Team, Handler)
            .Produces<TeamSuggestion>()
            .WithSummary("Get a single team suggestion")
            .WithName("Get team suggestion")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITeamSuggestionRepository repository,
        [FromRoute] Guid teamId,
        CancellationToken ct
    )
    {
        var result =
            await repository.GetTeamSuggestion(user.GetOrganization, teamId, ct)
            ?? throw new NotFoundException("Team", teamId);

        return TypedResults.Ok(result);
    }
}
