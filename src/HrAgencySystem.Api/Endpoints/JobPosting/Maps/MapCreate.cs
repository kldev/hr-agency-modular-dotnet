using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/recruitment/job-posting
        group
            .MapPost(ApiEndpoints.Recruitment.JobPosts.Create, Handler)
            .WithSummary("Create job post")
            .WithName("Create job post")
            .Produces<JobPostCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        CreatePostRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<JobPostCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );

        var integrationEvent = new JobPostCreatedIntegrationEvent(
            result.CompanyId,
            result.JobPostId
        );
        await bus.PublishAsync(integrationEvent);

        return TypedResults.Created($"/api/recruitment/job-posting/{result.JobPostId}", result);
    }
}

internal sealed record CreatePostRequest(
    [property: Description("The job description - the position - this post advertises.")]
        Guid JobDescriptionId,
    [property: Description("The title candidates see - it may differ from the job description's.")]
        string Title,
    [property: Description("Optional one-paragraph teaser for lists and job boards.")]
        string? Summary,
    [property: Description(
        "The candidate-facing text. Deliberately its own wording, not a copy of the internal description."
    )]
        string Description,
    [property: Description("What the person will do, one item per entry.")]
        IReadOnlyList<string> Responsibilities,
    [property: Description("What the person must bring, one item per entry.")]
        IReadOnlyList<string> Requirements,
    [property: Description("Skills listed on the post, e.g. [\"C#\", \"Docker\"].")]
        IReadOnlyList<string> Skills,
    [property: Description(
        "Where the work is, as candidates should read it, e.g. \"Opole\" or \"Remote\"."
    )]
        string Location,
    [property: Description("Country of the work, ISO 3166-1 alpha-2.")] string CountryCode,
    [property: Description(
        "The language the post is written in, two letters, e.g. \"PL\", \"EN\". Several posts of one job description usually differ by this."
    )]
        string LanguageCode,
    [property: Description("FullTime, PartTime, Contract, Temporary or Internship.")]
        EmploymentType EmploymentType,
    [property: Description("OnSite, Hybrid or Remote.")] WorkMode WorkMode,
    [property: Description("Currency of the salary range: PLN, EUR, USD or GBP.")]
        CurrencyCode CurrencyCode,
    [property: Description(
        "Bottom of the advertised salary range. Neither may be negative and the minimum cannot exceed the maximum; both in CurrencyCode."
    )]
        decimal SalaryMin,
    [property: Description(
        "Top of the advertised salary range. Neither may be negative and the minimum cannot exceed the maximum; both in CurrencyCode."
    )]
        decimal SalaryMax,
    [property: Description("The recruiter who owns the post.")] Guid RecruiterId
)
{
    public CreateJobPost ToCommand(Guid organizationId, Guid createdBy) =>
        new(
            JobDescriptionId,
            organizationId,
            Title,
            Summary,
            Description,
            Responsibilities,
            Requirements,
            Skills,
            Location,
            CountryCode,
            LanguageCode,
            EmploymentType,
            WorkMode,
            CurrencyCode,
            SalaryMin,
            SalaryMax,
            RecruiterId,
            createdBy
        );
}
