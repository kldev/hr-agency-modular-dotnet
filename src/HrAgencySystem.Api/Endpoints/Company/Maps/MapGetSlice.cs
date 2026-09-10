using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetSlice
{
    internal static void Map(
        RouteGroupBuilder endpoints)
    {
        // GET /api/companies
        endpoints.MapGet("", Handler)
            .WithSummary("Get companies")
            .Produces<SliceResponse<CompanyProjection>>()
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }
    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompaniesQueryRepository repository,
        string? search,
        int page = 1, int pageSize = 100)
    {
        return TypedResults.Ok(await repository.GetCompanies(search ?? "", user.OrganizationId, page, pageSize));
    }
}