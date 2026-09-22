using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.JobDescription.Application.Create;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.JobDescriptions.Create, Handler)
            .Produces<JobDescriptionCreated>()
            .ProducesStandardErrors()
            .WithSummary("Create job description")
            .WithName("Create job description");
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        CreateJobDescriptionRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<JobDescriptionCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );
        return TypedResults.Created($"/api/job-description/{result.JobDescriptionId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CreateJobDescriptionRequest(
    [property: Description("The client company the position is for.")] Guid CompanyId,
    [property: Description(
        "The position, e.g. \"Senior .NET Developer\". One job description is one position."
    )]
        string Title,
    [property: Description("Optional one-paragraph summary for lists.")] string? Summary,
    [property: Description(
        "The internal description of the role - what the client wants. The candidate-facing wording belongs to job posts."
    )]
        string Description,
    [property: Description("What the person will do, one item per entry.")]
        IReadOnlyList<string> Responsibilities,
    [property: Description("What the person must bring, one item per entry.")]
        IReadOnlyList<string> Requirements,
    [property: Description("Skills to match candidates on, e.g. [\"C#\", \"PostgreSQL\"].")]
        IReadOnlyList<string> Skills,
    [property: Description("Where the work is, e.g. \"Warszawa\" or \"Remote\".")] string Location,
    [property: Description("Country of the work, ISO 3166-1 alpha-2.")] string CountryCode,
    [property: Description("FullTime, PartTime, Contract, Temporary or Internship.")]
        EmploymentType EmploymentType,
    [property: Description("OnSite, Hybrid or Remote.")] WorkMode WorkMode,
    [property: Description("Currency of the salary range: PLN, EUR, USD or GBP.")]
        CurrencyCode CurrencyCode,
    [property: Description(
        "Bottom of the salary range. Salary range: SalaryMin and SalaryMax, in CurrencyCode. Neither may be negative and the minimum cannot exceed the maximum."
    )]
        decimal SalaryMin,
    [property: Description(
        "Top of the salary range. Salary range: SalaryMin and SalaryMax, in CurrencyCode. Neither may be negative and the minimum cannot exceed the maximum."
    )]
        decimal SalaryMax,
    [property: Description("The recruiter responsible for filling the position.")] Guid RecruiterId
)
{
    public CreateJobDescription ToCommand(Guid organizationId, Guid createdBy) =>
        new(
            organizationId,
            CompanyId,
            Title,
            Summary,
            Description,
            Responsibilities,
            Requirements,
            Skills,
            Location,
            CountryCode,
            EmploymentType,
            WorkMode,
            CurrencyCode,
            SalaryMin,
            SalaryMax,
            RecruiterId,
            createdBy
        );
}
