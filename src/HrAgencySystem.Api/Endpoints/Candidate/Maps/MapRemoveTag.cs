using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Candidate.RemoveCandidateTag;
using HrAgencySystem.Recruitment.Application.Candidate.TagCandidate;
using HrAgencySystem.Recruitment.Events.Candidate;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapRemoveTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates/{id}/tag
        group.MapDelete("{candidateId:guid}/tag/{tagId:guid}", Handler).WithSummary("Delete candidate tag")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);
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