using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetSlice
{
    internal static void Map(
        RouteGroupBuilder endpoints)
    {
        // GET /api/companies
        endpoints.MapGet("", Handler)
            .WithSummary("Get companies")
            .WithName("Get companies")
            .Produces<SliceResponse<CompanyProjection>>()
            .ProducesStandardErrors();
    }
    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompaniesQueryRepository repository,
        string? search,
        int page = 1, int pageSize = 100)
    {
        return TypedResults.Ok(await repository.GetCompanies(search ?? "", user.OrganizationId, page, pageSize));
    }
}