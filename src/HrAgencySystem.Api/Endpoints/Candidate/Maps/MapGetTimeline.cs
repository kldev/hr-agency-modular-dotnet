using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Timeline.Queries;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapGetTimeline
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/candidates/{id}/timeline
        group
            .MapGet(ApiEndpoints.Recruitment.Candidates.Timeline, Handler)
            .WithSummary("Get candidate timeline")
            .WithName("Get candidate timeline")
            .Produces<TimelineSlice>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITimelineQueryRepository repository,
        Guid candidateId,
        string? after,
        int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetCandidateTimeline(
            user.OrganizationId,
            candidateId,
            TimelineCursor.Parse(after),
            pageSize,
            ct
        );

        return TypedResults.Ok(result);
    }
}
