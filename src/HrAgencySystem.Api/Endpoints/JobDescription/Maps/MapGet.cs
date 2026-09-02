using System.Net;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/job-description/{jobDescriptionId:guid}", Handler).WithSummary("Get job description");
    }

    private static async Task<IResult> Handler(IDocumentSession session, AppUserAuthenticated user, Guid jobDescriptionId, CancellationToken ct)
    {
        var result = await session.Query<JobDescriptionProjection>()
            .Where(z => z.Id == jobDescriptionId && z.OrganizationId == user.OrganizationId).FirstOrDefaultAsync(ct);

        if (result == null)
        {
            return TypedResults.NotFound(new ProblemDetails()
            {
                Title = "Not found",
                Detail = $"Job description with id {jobDescriptionId} not found", Status = (int)HttpStatusCode.NotFound
            });
        }
        
        return TypedResults.Ok(result);
    }
}