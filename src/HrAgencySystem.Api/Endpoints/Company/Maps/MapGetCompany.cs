using System.Net;
using HrAgencySystem.Company.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetCompany
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/companies/{companyId:guid}", Handler);
    }
    
    private static async Task<IResult> Handler(IDocumentSession session, Guid companyId, [FromQuery]Guid organizationId, CancellationToken ct)
    {
        var result = await session.Query<CompanyProjection>()
            .Where(z => z.Id == companyId && z.OrganizationId == organizationId)
            .FirstOrDefaultAsync(ct);

        if (result == null)
        {
            return TypedResults.NotFound(new ProblemDetails()
            {
                Title = "Not found",
                Detail = $"Company with id {companyId} not found", Status = (int)HttpStatusCode.NotFound
            });
        }

        return TypedResults.Ok(result);
    }
}