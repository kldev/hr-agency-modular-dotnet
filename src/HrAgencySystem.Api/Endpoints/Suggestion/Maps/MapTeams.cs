using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Teams.Application.Suggestion;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapTeams
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Teams, Handler)
            .Produces<IReadOnlyList<TeamSuggestion>>()
            .WithSummary("Get top 25 teams")
            .WithName("Get teams suggestions")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITeamSuggestionRepository repository,
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        var result = await repository.GetTeamSuggestions(user.GetOrganization, search ?? "", ct);

        return TypedResults.Ok(result);
    }
}
