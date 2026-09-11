using HrAgencySystem.Api.Common;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", Handler)
            .WithSummary("Get organizations")
            .Produces<SliceResponse<OrganizationProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IOrganizationQueryRepository repository,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await repository.GetSlice(page, pageSize, ct);

        return TypedResults.Ok(result);
    }
}