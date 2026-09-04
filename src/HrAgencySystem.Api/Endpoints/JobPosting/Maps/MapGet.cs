using System.Net;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/recruitment/job-posting/{jobPostId:guid}", Handler).WithSummary("Get job post");
    }

    private static async Task<IResult> Handler(IQuerySession session, AppUserAuthenticated user, Guid jobPostId, CancellationToken ct)
    {
        var result = await session.Query<JobPostProjection>()
            .Where(z => z.Id == jobPostId && z.OrgId == user.OrganizationId).FirstOrDefaultAsync(ct);

        if (result == null)
        {
            return TypedResults.NotFound(new ProblemDetails()
            {
                Title = "Not found",
                Detail = $"Job post with id {jobPostId} not found", Status = (int)HttpStatusCode.NotFound
            });
        }
        
        return TypedResults.Ok(result);
    }
}