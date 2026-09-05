using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.JobApplication.Create;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Events.JobApplication;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapApplyTo
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/recruitment/job-posting/{jobPostId}/apply
        group.MapPost("/{jobPostId:guid}/apply", Handler).WithSummary("Create candidate application to job post");
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
    public CreateJobApplication ToCommand(Guid jobPostId)
    {
        return new CreateJobApplication(jobPostId,  Guid.NewGuid(), Email, PhoneNumber, Source, FirstName, LastName);
    }
}
 