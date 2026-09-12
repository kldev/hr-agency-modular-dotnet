using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.JobDescription.Application.Port;
using HrAgencySystem.JobDescription.Projections;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/job-description/{jobDescriptionId:guid}", Handler)
            .Produces<JobDescriptionProjection>()
            .ProducesStandardErrors()
            .WithSummary("Get job description")
            .WithName("Get job description");
    }

    private static async Task<IResult> Handler(IJobDescriptionQueryRepository repository, AppUserAuthenticated user, Guid jobDescriptionId, CancellationToken ct)
    {
        var result = await repository.GetJobDescription(user.OrganizationId, jobDescriptionId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Job description", jobDescriptionId));
        }
        
        return TypedResults.Ok(result);
    }
}