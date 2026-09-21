using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Suggestion;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapWorker
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Worker, Handler)
            .Produces<WorkerSuggestion>()
            .WithName("Get worker suggestion")
            .WithSummary("Get a single worker suggestion by id")
            .ProducesStandardErrors();
    }

    // A worker from another organization answers 404 rather than 403: the picker has no business
    // learning that the id exists.
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IWorkerSuggestionRepository repository,
        Guid workerId,
        CancellationToken ct
    )
    {
        var result = await repository.ById(user.GetOrganization, workerId, ct);

        if (result is null)
            throw new NotFoundException("Worker", workerId);

        return TypedResults.Ok(result);
    }
}
