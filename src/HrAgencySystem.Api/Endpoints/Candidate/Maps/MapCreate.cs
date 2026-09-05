using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Events.Candidate;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates
        group.MapPost("", Handler).WithSummary("Creates a new candidate")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, CreateCandidateRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CandidateCreated>(request.ToCommand(user.OrganizationId, user.UserId), ct);
        return TypedResults.Created($"/api/recruitment/candidates/{result.CandidateId}", result);
    }
}

internal sealed record CreateCandidateRequest(string Email, string PhoneNumber, string FirstName, string LastName, CandidateSource Source)
{
    public CreateCandidate ToCommand(Guid organizationId, Guid createdBy)
        => new CreateCandidate(organizationId, Email, Source, PhoneNumber, FirstName, LastName, createdBy);
}