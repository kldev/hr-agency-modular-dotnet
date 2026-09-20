using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/workers
        endpoints
            .MapGet(ApiEndpoints.Workers.Slice, Handler)
            .WithSummary("Get a slice of workers")
            .WithName("Get workers")
            .ProducesStandardErrors()
            .Produces<SliceResponse<WorkerProjection>>();
    }

    /// <summary>
    /// <paramref name="workCountry"/> and <paramref name="excludeWorkCountry"/> are what the two
    /// halves of the office point their screens at: one list for the people working here, another
    /// for the ones abroad. Which country is which is the caller's business - the API takes
    /// countries, not a notion of home.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IWorkersQueryRepository repository,
        [FromQuery] string? search,
        [FromQuery] WorkerStatus[]? status,
        [FromQuery] ResponsibleDepartment[]? department,
        [FromQuery] string[]? workCountry,
        [FromQuery] string[]? excludeWorkCountry,
        [FromQuery] string? citizenship,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetWorkers(
            user.GetOrganization,
            new WorkerQuery(
                search ?? "",
                status,
                department,
                workCountry,
                excludeWorkCountry,
                citizenship,
                page,
                pageSize
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
