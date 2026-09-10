using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/interviews
        group.MapGet("{interviewId}", Handler)
            .WithSummary("Get interview")
            .Produces<InterviewProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IInterviewsQueryRepository repository, Guid interviewId, CancellationToken ct)
    {
        var result = await repository.Get(user.OrganizationId, interviewId, ct);
        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Interview", interviewId));
        }

        return TypedResults.Ok(result);
    }
}