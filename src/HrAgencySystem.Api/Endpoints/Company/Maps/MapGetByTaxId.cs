using System.Net;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetByTaxId
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/companies/find-by-tax/{taxId}", Handler).WithSummary("Get company by tax id");
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