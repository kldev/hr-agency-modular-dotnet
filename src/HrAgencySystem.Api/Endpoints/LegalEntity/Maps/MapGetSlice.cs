using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.LegalEntity.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/legal-entities
        endpoints
            .MapGet(ApiEndpoints.LegalEntities.Slice, Handler)
            .WithSummary("Get legal entities")
            .WithName("Get legal entities")
            .Produces<SliceResponse<LegalEntityProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ILegalEntitiesQueryRepository repository,
        IClock clock,
        [FromQuery] string? search,
        [FromQuery] bool activeOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetLegalEntities(
            user.GetOrganization,
            search ?? "",
            activeOnly,
            DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
            page,
            pageSize,
            ct
        );

        return TypedResults.Ok(result);
    }
}
