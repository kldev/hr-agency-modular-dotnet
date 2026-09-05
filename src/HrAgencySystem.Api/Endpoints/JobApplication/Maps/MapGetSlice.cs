using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.JobApplication;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/job-applications
        group.MapGet("", Handler).WithSummary("Get job applications");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IJobApplicationQueryRepository repository,
        string? search, 
        Guid? companyId, 
        Guid[]? tag,
        JobApplicationStatus[]? status,
        CandidateSource[]? source,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new JobApplicationQuery(search ?? "", companyId, tag ?? [], status, source ?? [], page, pageSize);
        var result = await repository.GetJobApplications(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}