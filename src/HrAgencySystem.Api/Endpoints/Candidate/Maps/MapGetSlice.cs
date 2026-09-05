using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates
        group.MapGet("", Handler).WithSummary("Get candidates");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, ICandidateQueryRepository repository,
        string? search, 
        Guid? companyId, 
        Guid[]? tag,
        CandidateStatus? status,
        CandidateSource[]? source,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new CandidateQuery(search ?? "", companyId, tag ?? [], status, source ?? [], page, pageSize);
        var result = await repository.GetCandidates(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}