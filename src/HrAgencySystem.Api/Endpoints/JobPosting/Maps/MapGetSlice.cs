using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Posting;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/recruitment/job-posting", Handler).WithSummary("Get job posts");
    }

    private static async Task<IResult> Handler(IJobPostQueryRepository repository,
        AppUserAuthenticated user,
        string? search,
        Guid? companyId,
        Guid? recruiterId,
        JobPostStatus[]? status,
        string[]? lang,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new JobPostQuery(search ?? "", companyId, recruiterId, status ?? [], lang ?? [], page, pageSize);
        var result = await repository.GetJobPosts(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}