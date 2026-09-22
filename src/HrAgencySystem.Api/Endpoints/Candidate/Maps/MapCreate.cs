using System.ComponentModel;
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
        group
            .MapPost(ApiEndpoints.Recruitment.Candidates.Create, Handler)
            .WithSummary("Create candidate")
            .WithName("Create candidate")
            .ProducesStandardErrors()
            .Produces<CandidateCreated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        CreateCandidateRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CandidateCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );
        return TypedResults.Created($"/api/recruitment/candidates/{result.CandidateId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CreateCandidateRequest(
    [property: Description(
        "The candidate's e-mail address - unique within the agency, one person is one candidate."
    )]
        string Email,
    [property: Description("The candidate's phone number.")] string PhoneNumber,
    [property: Description("First name.")] string FirstName,
    [property: Description("Last name.")] string LastName,
    [property: Description(
        "Where the candidate came from, e.g. Direct, Referral, Sourcing, JustJoinIt, Linkedin."
    )]
        CandidateSource Source,
    [property: Description("Free notes about the candidate. May be empty.")] string Note
)
{
    public CreateCandidate ToCommand(Guid organizationId, Guid createdBy) =>
        new(organizationId, Email, Source, PhoneNumber, FirstName, LastName, createdBy, null, Note);
}
