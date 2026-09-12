using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Candidates.TagCandidate;
using HrAgencySystem.Recruitment.Events.Candidates;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates/{id}/tag
        group.MapPut("{candidateId:guid}/tag", Handler)
            .WithSummary("Tag candidate")
            .WithName("Tag candidate")
            .ProducesStandardErrors()
            .Produces<CandidateTagged>();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid candidateId,TagRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CandidateTagged>(new TagCandidate(request.TagId, candidateId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}

