using HrAgencySystem.Company.Application.Port;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetCompanies
{
    public static void Map(
        RouteGroupBuilder endpoints)
    {
        endpoints.MapGet("", Handler).WithSummary("Get Companies");
    }
    private static async Task<IResult> Handler(ICompaniesQueryRepository repository,
        string? search,
        Guid organizationId,
        int page = 1, int pageSize = 100)
    {
        return TypedResults.Ok(await repository.GetCompanies(search ?? "", organizationId, page, pageSize));
    }
}