using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Candidates.Create;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates
        group.MapPost("", Handler).WithSummary("Create candidate")
            .ProducesStandardErrors()
            .Produces<CandidateCreated>();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, CreateCandidateRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CandidateCreated>(
            request.ToCommand(user.OrganizationId, user.UserId), ct);
        return TypedResults.Created($"/api/recruitment/candidates/{result.CandidateId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CreateCandidateRequest(
    string Email, 
    string PhoneNumber, 
    string FirstName, 
    string LastName, 
    CandidateSource Source,
    string Note)
{
    public CreateCandidate ToCommand(Guid organizationId, Guid createdBy)
        => new (organizationId, 
            Email, 
            Source, 
            PhoneNumber, 
            FirstName, 
            LastName, 
            createdBy, 
            null, 
            Note);
}