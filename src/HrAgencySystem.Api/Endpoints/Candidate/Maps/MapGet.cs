using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.Candidates.Queries;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates
        group.MapGet("{candidateId:guid}", Handler).WithSummary("Get candidate")
            .Produces<CandidateProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        ICandidateQueryRepository repository, 
        Guid candidateId, CancellationToken ct)
    {
        var result = await repository.GetCandidate(user.OrganizationId, candidateId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Candidate", candidateId));
        }

        return TypedResults.Ok(result);
    }
}