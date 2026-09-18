using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Candidates.RemoveCandidateTag;
using HrAgencySystem.Recruitment.Events.Candidates;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapRemoveTagList
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates/{id}/tag
        group
            .MapPut("{candidateId:guid}/tag/remove", Handler)
            .WithSummary("Remove tag list")
            .WithName("Remove candidate tag list")
            .ProducesStandardErrors()
            .Produces<CandidateTagRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid candidateId,
        TagRequestList request,
        CancellationToken ct
    )
    {
        CandidateTagRemoved? result = null;
        foreach (var tag in request.TagIds)
        {
            result = await bus.InvokeAsync<CandidateTagRemoved>(
                new RemoveCandidateTag(candidateId, tag, user.OrganizationId, user.UserId),
                ct
            );
        }

        return TypedResults.Ok(result);
    }
}
