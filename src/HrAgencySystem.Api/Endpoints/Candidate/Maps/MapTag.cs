using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.Candidate.TagCandidate;
using HrAgencySystem.Recruitment.Events.Candidates;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates/{id}/tag
        group.MapPut("{candidateId:guid}/tag", Handler).WithSummary("Add candidate tag")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid candidateId,TagRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CandidateTagged>(new TagCandidate(request.TagId, candidateId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}

