using HrAgencySystem.Organization.Application.Port;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/organization", Handler).WithSummary("Get organizations");
    }

    private static async Task<IResult> Handler(IOrganizationQueryRepository repository,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await repository.GetSlice(page, pageSize, ct);

        return TypedResults.Ok(result);
    }
}