using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/job-applications/{id}
        group.MapGet("{jobApplicationId:guid}", Handler)
            .WithSummary("Get application")
            .WithName("Get job application")
            .Produces<JobApplicationProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IJobApplicationQueryRepository repository,
        Guid jobApplicationId, CancellationToken ct)
    {
        var result = await repository.GetJobApplication(user.OrganizationId, jobApplicationId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Job Application", jobApplicationId));
        }

        return TypedResults.Ok(result);
    }
}