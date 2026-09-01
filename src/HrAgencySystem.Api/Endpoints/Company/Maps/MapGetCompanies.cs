using HrAgencySystem.Api.Auth;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.SharedKernel.Port;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetCompanies
{
    internal static void Map(
        RouteGroupBuilder endpoints)
    {
        endpoints.MapGet("/api/companies", Handler).WithSummary("Get Companies");
    }
    private static async Task<IResult> Handler(AuthenticatedUser user, ICompaniesQueryRepository repository,
        string? search,
        int page = 1, int pageSize = 100)
    {
        return TypedResults.Ok(await repository.GetCompanies(search ?? "", user.OrganizationId, page, pageSize));
    }
}