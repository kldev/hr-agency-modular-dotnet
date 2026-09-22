using System.ComponentModel;
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
        group
            .MapPost(ApiEndpoints.Recruitment.JobPosts.ApplyTo, Handler)
            .Produces<JobApplicationCreated>()
            .ProducesStandardErrors()
            .WithSummary("Apply to job post")
            .WithName("Apply to job post");
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid jobPostId,
        ApplyToPostRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<JobApplicationCreated>(request.ToCommand(jobPostId), ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record ApplyToPostRequest(
    [property: Description(
        "The candidate's e-mail address. An existing candidate with this address is reused, so one person stays one candidate."
    )]
        string Email,
    [property: Description("The candidate's phone number.")] string PhoneNumber,
    [property: Description(
        "Where the candidate came from, e.g. Direct, Referral, JustJoinIt, Linkedin. Defaults to Direct."
    )]
        CandidateSource Source = CandidateSource.Direct,
    [property: Description("The candidate's first name. Optional.")] string FirstName = "",
    [property: Description("The candidate's last name. Optional.")] string LastName = ""
)
{
    public ApplyToJobApplication ToCommand(Guid jobPostId)
    {
        return new ApplyToJobApplication(
            jobPostId,
            Email,
            PhoneNumber,
            Source,
            FirstName,
            LastName
        );
    }
}
