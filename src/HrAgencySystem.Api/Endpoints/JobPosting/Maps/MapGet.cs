using System.Net;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.Port;
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

    private static async Task<IResult> Handler(IJobPostQueryRepository repository, AppUserAuthenticated user, Guid jobPostId, CancellationToken ct)
    {
        var result = await repository.GetJobPost(user.OrganizationId, jobPostId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Job post", jobPostId));
        }
        
        return TypedResults.Ok(result);
    }
}