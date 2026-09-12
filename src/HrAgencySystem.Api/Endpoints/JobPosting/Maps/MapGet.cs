using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Projections;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET/api/recruitment/job-posting/{{id}
        group.MapGet("{jobPostId:guid}", Handler)
            .WithSummary("Get job post")
            .WithName("Get job post")
            .Produces<JobPostProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IJobPostQueryRepository repository, IOptions<ApplicationConfig> config, AppUserAuthenticated user, Guid jobPostId, CancellationToken ct)
    {
        var result = await repository.GetJobPost(user.OrganizationId, jobPostId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Job post", jobPostId));
        }
        
        return TypedResults.Ok(result with
        {
            PostingSlug = $"{config.Value}/{result.PostingSlug}"
        });
    }
}