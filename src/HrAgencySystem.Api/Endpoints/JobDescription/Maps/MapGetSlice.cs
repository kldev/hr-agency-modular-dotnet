using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/job-description", Handler).WithSummary("Get job descriptions");
    }
    
    private static async Task<IResult> Handler(IDocumentSession session, AppUserAuthenticated user, CancellationToken ct)
    {
        var result = await session.Query<JobDescriptionProjection>()
            .Where(z => z.OrganizationId == user.OrganizationId).ToListAsync(ct);
        
        return TypedResults.Ok(result);
    }
}