using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Candidates.RemoveCandidateTag;
using HrAgencySystem.Recruitment.Events.Candidates;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapRemoveTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates/{id}/tag
        group.MapDelete("{candidateId:guid}/tag/{tagId:guid}", Handler)
            .WithSummary("Remove tag")
            .ProducesStandardErrors()
            .Produces<CandidateTagRemoved>();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid candidateId,
        Guid tagId,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CandidateTagRemoved>(new RemoveCandidateTag(candidateId, tagId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}