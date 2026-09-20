using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/workers/{workerId}
        endpoints
            .MapGet(ApiEndpoints.Workers.Get, Handler)
            .WithSummary("Get a worker")
            .WithName("Get worker")
            .ProducesStandardErrors()
            .Produces<WorkerProjection>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IWorkersQueryRepository repository,
        [FromRoute] Guid workerId,
        CancellationToken ct
    )
    {
        // A file in another organization answers 404, never 403: a different answer would confirm
        // that the id exists somewhere.
        var worker =
            await repository.GetWorker(user.GetOrganization, workerId, ct)
            ?? throw new NotFoundException("Worker", workerId);

        return TypedResults.Ok(worker);
    }
}
