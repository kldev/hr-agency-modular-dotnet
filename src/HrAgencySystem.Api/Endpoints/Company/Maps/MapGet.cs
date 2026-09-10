using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/companies/{companyId:guid}
        group.MapGet("{companyId:guid}", Handler)
            .WithSummary("Get company")
            .Produces<CompanyProjection>()
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompaniesQueryRepository repository, Guid companyId,
        CancellationToken ct)
    {
        var result = await repository.GetCompany(user.OrganizationId, companyId, "", ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Company", companyId));
        }
        
        return TypedResults.Ok(result);
    }
}