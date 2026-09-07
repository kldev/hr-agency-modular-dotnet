using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Domain.Interviews;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/interviews
        group.MapGet("", Handler).WithSummary("Get interviews");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IInterviewsQueryRepository repository,
        Guid?  jobApplicationId,
        Guid? interviewerId,
        Guid? candidateId, 
        Guid? createdByUserId,
        InterviewStatus? status,
        DateOnly? fromDate,
        DateOnly? toDate,
        string timezone = "Europe/Warsaw",
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new InterviewsQuery(
            jobApplicationId,
            candidateId,
            createdByUserId,
            interviewerId,
            status,
            fromDate.ToUtc(timezone),
            toDate?.AddDays(1).ToUtc(timezone),
            page, pageSize);
        var result = await repository.GetSlice(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}