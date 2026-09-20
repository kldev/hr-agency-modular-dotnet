using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Workers.Application.Suggestion;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapWorkers
{
    private const int Limit = 25;

    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Workers, Handler)
            .Produces<IReadOnlyList<WorkerSuggestion>>()
            .WithSummary("Get top 25 workers")
            .WithName("Get worker suggestions")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IWorkerSuggestionRepository repository,
        [FromQuery] string? search,
        CancellationToken ct
    ) => TypedResults.Ok(await repository.Search(user.GetOrganization, search ?? "", Limit, ct));
}
