using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Suggestion;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapPositions
{
    private const int Limit = 25;

    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Positions, Handler)
            .Produces<IReadOnlyList<PositionSuggestion>>()
            .WithSummary("Get top 25 positions, optionally within one project")
            .WithName("Get position suggestions")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IPositionSuggestionRepository repository,
        [FromQuery] Guid? projectId,
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        var result = await repository.Search(
            user.GetOrganization,
            projectId,
            search ?? "",
            Limit,
            ct
        );

        return TypedResults.Ok(result);
    }
}
