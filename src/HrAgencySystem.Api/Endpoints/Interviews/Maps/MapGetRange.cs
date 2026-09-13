
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapGetRange
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/interviews/range
        group.MapGet("range", Handler)
            .WithSummary("Get interviews for date range")  
            .WithName("Get interviews for date range")
            .Produces<IReadOnlyList<InterviewProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IInterviewsQueryRepository repository,
        string? search,
        Guid?  jobApplicationId,
        Guid? interviewerId,
        Guid? candidateId, 
        Guid? createdByUserId,
        InterviewStatus? status,
        DateOnly fromDate,
        DateOnly toDate,
        string timezone = "Europe/Warsaw",
        CancellationToken ct = default)
    {
        var query = new InterviewsQuery(
            jobApplicationId,
            candidateId,
            createdByUserId,
            interviewerId,
            status,
            fromDate.ToUtc(timezone),
            toDate.AddDays(1).ToUtc(timezone),
            search ?? "",
            1, 1); // ignored in this query
        var result = await repository.GetRange(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}