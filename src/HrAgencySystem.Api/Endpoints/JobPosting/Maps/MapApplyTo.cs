using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobApplications.Create;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapApplyTo
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/recruitment/job-posting/{jobPostId}/apply
        group.MapPost("/{jobPostId:guid}/apply", Handler)
            .Produces<JobApplicationCreated>()
            .ProducesStandardErrors()
            .WithSummary("Apply to job post");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, Guid jobPostId, ApplyToPostRequest request,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobApplicationCreated>(request.ToCommand(jobPostId), ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record ApplyToPostRequest(string Email, string PhoneNumber, CandidateSource Source = CandidateSource.Direct, string FirstName = "", string LastName = "")
{
    public ApplyToJobApplication ToCommand(Guid jobPostId)
    {
        return new ApplyToJobApplication(jobPostId,  Email, PhoneNumber, Source, FirstName, LastName);
    }
}
 