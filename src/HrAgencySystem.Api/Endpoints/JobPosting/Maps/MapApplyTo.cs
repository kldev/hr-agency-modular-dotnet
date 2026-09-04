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
        group.MapPost("/api/recruitment/job-posting/{jobPostId}/apply", Handler).WithSummary("Create candidate application to job post");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, ApplyToPostRequest request,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobApplicationCreated>(request.ToCommand(), ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record ApplyToPostRequest(Guid JobPostId, string Email, string PhoneNumber, CandidateSource Source = CandidateSource.Direct)
{
    public CreateJobApplication ToCommand()
    {
        return new CreateJobApplication(JobPostId, Email, PhoneNumber, Source);
    }
}
 