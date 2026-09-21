using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Position.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/positions - the register across projects, filtered by one of them when asked.
        endpoints
            .MapGet(ApiEndpoints.Positions.Slice, Handler)
            .WithSummary("Get positions")
            .WithName("Get positions")
            .ProducesStandardErrors()
            .Produces<SliceResponse<PositionListItem>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IPositionsQueryRepository repository,
        [FromQuery] string? search,
        [FromQuery] Guid? projectId,
        [FromQuery] WorkerContractType? contractType,
        [FromQuery] bool includeArchived = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetPositions(
            user.GetOrganization,
            new PositionQuery(
                search ?? "",
                projectId,
                includeArchived,
                contractType,
                page,
                pageSize
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
