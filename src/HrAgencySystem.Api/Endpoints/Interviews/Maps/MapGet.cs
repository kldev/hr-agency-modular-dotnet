using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/interviews
        group.MapGet("{interviewId}", Handler)
            .WithSummary("Get interview")
            .Produces<InterviewProjection>()
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
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