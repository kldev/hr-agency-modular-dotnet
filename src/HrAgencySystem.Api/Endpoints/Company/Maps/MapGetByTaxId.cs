using System.Net;
using HrAgencySystem.Api.Auth;
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
    
    private static async Task<IResult> Handler(AppUserAuthenticated user, IDocumentSession session, string taxId, CancellationToken ct)
    {
        var result = await session.Query<CompanyProjection>()
            .Where(z => z.TaxId == taxId && z.OrganizationId == user.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (result == null)
        {
            return TypedResults.NotFound(new ProblemDetails()
            {
                Title = "Not found",
                Detail = $"Company with tax {taxId} not found", Status = (int)HttpStatusCode.NotFound
            });
        }

        return TypedResults.Ok(result);
    }
}