using System.Net;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Company.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/companies/{companyId:guid}", Handler).WithSummary("Get company");
    }
    
    private static async Task<IResult> Handler(AppUserAuthenticated user, IDocumentSession session, Guid companyId, CancellationToken ct)
    {
        var result = await session.Query<CompanyProjection>()
            .Where(z => z.Id == companyId && z.OrganizationId == user.OrganizationId)
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