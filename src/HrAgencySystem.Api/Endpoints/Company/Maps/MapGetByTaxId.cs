using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetByTaxId
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/companies/find-by-tax/{taxId}
        group.MapGet("find-by-tax/{taxId}", Handler)
            .WithSummary("Get company by tax")
            .WithName("Get company by tax")
            .Produces<CompanyProjection>()
            .ProducesStandardErrors();
    }
    
    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompaniesQueryRepository repository, string taxId, CancellationToken ct)
    {
        var result = await repository.GetCompany(user.OrganizationId, null, taxId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Company", taxId));
        }

        return TypedResults.Ok(result);
    }
}