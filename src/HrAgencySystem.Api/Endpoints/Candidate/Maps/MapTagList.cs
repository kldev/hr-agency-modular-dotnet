using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Candidates.TagCandidate;
using HrAgencySystem.Recruitment.Events.Candidates;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapTagList
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUI api/recruitment/candidates/{id}/tag-list
        group.MapPut("{candidateId:guid}/tag-list", Handler)
            .WithSummary("Add multiple tag")
            .WithName("Add multiple tag to candidate")
            .Produces<CandidateTagged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid candidateId,
        TagRequestList request,
        CancellationToken ct)
    {
        CandidateTagged? result = null;
        foreach (var tag in request.TagIds)
        {
            result =
                await bus.InvokeAsync<CandidateTagged>(
                    new TagCandidate(tag, candidateId, user.OrganizationId, user.UserId), ct);
        }

        return TypedResults.Ok(result);
    }
}