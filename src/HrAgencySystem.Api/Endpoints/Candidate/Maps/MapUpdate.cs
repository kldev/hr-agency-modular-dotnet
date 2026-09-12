using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Candidates.Update;
using HrAgencySystem.Recruitment.Events.Candidates;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Candidate.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{candidateId:guid}", Handler)
            .WithSummary("Update candidate")
            .WithName("Update candidate")
            .ProducesStandardErrors()
            .Produces<CandidateUpdated>();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus,
        Guid candidateId,
        UpdateCandidateRequest request,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CandidateUpdated>(
                request.ToCommand(user.OrganizationId, candidateId, user.UserId), 
                ct);
        return TypedResults.Ok(result);
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed record UpdateCandidateRequest(
        string Phone, 
        string FirstName, 
        string LastName, 
        string Note)
    {
        public UpdateCandidate ToCommand(Guid organizationId,Guid candidateId, Guid modifiedBy)
            => new (
                candidateId, 
                organizationId,
                Phone, 
                FirstName, 
                LastName, 
                Note, 
                modifiedBy);
    }
}
