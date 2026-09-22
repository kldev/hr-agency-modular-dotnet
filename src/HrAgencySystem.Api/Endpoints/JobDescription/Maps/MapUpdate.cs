using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.JobDescription.Application.Update;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.JobDescriptions.Update, Handler)
            .ProducesStandardErrors()
            .Produces<JobDescriptionUpdated>()
            .WithSummary("Update job description")
            .WithName("Update job description");
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid jobDescriptionId,
        UpdateJobDescriptionRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<JobDescriptionUpdated>(
            request.ToCommand(jobDescriptionId, user.OrganizationId, user.UserId),
            ct
        );
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record UpdateJobDescriptionRequest(
    [property: Description("The position, e.g. \"Senior .NET Developer\".")] string Title,
    [property: Description("Optional one-paragraph summary for lists.")] string? Summary,
    [property: Description("The internal description of the role.")] string Description,
    [property: Description("What the person will do, one item per entry.")]
        IReadOnlyList<string> Responsibilities,
    [property: Description("What the person must bring, one item per entry.")]
        IReadOnlyList<string> Requirements,
    [property: Description("Skills to match candidates on.")] IReadOnlyList<string> Skills,
    [property: Description("Where the work is.")] string Location,
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
        decimal SalaryMax
)
{
    public UpdateJobDescription ToCommand(
        Guid jobDescriptionId,
        Guid organizationId,
        Guid modifiedBy
    ) =>
        new(
            jobDescriptionId,
            organizationId,
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
            modifiedBy
        );
}
