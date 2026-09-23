using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Timeline.Queries;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapGetTimeline
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/job-applications/{id}/timeline
        group
            .MapGet(ApiEndpoints.Recruitment.JobApplications.Timeline, Handler)
            .WithSummary("Get job application timeline")
            .WithName("Get job application timeline")
            .Produces<TimelineSlice>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITimelineQueryRepository repository,
        Guid jobApplicationId,
        string? after,
        int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetApplicationTimeline(
            user.OrganizationId,
            jobApplicationId,
            TimelineCursor.Parse(after),
            pageSize,
            ct
        );

        return TypedResults.Ok(result);
    }
}
