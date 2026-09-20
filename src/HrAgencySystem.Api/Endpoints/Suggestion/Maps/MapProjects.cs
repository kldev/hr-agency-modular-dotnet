using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Suggestion;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapProjects
{
    private const int Limit = 25;

    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Projects, Handler)
            .Produces<IReadOnlyList<ProjectSuggestion>>()
            .WithSummary("Get top 25 projects")
            .WithName("Get project suggestions")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IProjectSuggestionRepository repository,
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        var result = await repository.Search(user.GetOrganization, search ?? "", Limit, ct);

        return TypedResults.Ok(result);
    }
}
