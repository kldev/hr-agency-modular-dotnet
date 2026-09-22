using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.Positions.Open;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Position.Maps;

internal static class MapOpen
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/projects/{projectId}/positions - a role opens inside the project.
        endpoints
            .MapPost(ApiEndpoints.Projects.OpenPosition, Handler)
            .WithSummary("Open a position")
            .WithName("Open position")
            .ProducesStandardErrors()
            .Produces<ProjectPositionOpened>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        PositionRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectPositionOpened>(
            request.ToOpenCommand(projectId, user.GetOrganization, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}

/// <summary>
/// Everything a role says about itself, which is everything a contract for it will ask for. Shared
/// by opening and editing, because the two take the same shape - the same arrangement the project's
/// own create and update requests have.
/// </summary>
internal sealed record PositionRequest(
    [property: Description(
        "Internal name, unique within the project, e.g. \"Painter PL contract\" or \"Painter Belgium\" - it tells two roles apart."
    )]
        string Name,
    [property: Description(
        "The job title that goes on the contract, without internal bookkeeping, e.g. \"Painter\"."
    )]
        string? ContractName,
    [property: Description("What the work actually is.")] string? WorkDescription,
    [property: Description("Duties of the role, one item per entry.")]
        IReadOnlyList<string>? Duties,
    [property: Description("Qualifications the person must hold, e.g. a welding certificate.")]
        IReadOnlyList<string>? RequiredQualifications,
    [property: Description(
        "What is signed with the person: EmploymentContract, TemporaryEmploymentContract, MandateContract, SelfEmployed or Other. One project can hold several."
    )]
        WorkerContractType ContractType,
    [property: Description(
        "The role's proposed rate. What a given person actually gets belongs to their own contract. Omit for none."
    )]
        decimal? RateAmount,
    [property: Description(
        "Currency of RateAmount, e.g. \"PLN\", \"EUR\" - required whenever a rate is given."
    )]
        string? RateCurrency,
    [property: Description("What the rate is per: Hourly, Daily or Monthly.")] RateUnit RateUnit,
    [property: Description(
        "Gross or Net - without it the number reads differently to different people."
    )]
        RateBasis RateBasis,
    [property: Description(
        "Street. Workplace address of this position - street, building number, postal code, city and country all together, or none of them to use the project's own workplace."
    )]
        string? Street,
    [property: Description(
        "Building number. Workplace address of this position - street, building number, postal code, city and country all together, or none of them to use the project's own workplace."
    )]
        string? BuildingNumber,
    [property: Description("Optional unit number of the position's workplace.")] string? UnitNumber,
    [property: Description(
        "Postal code. Workplace address of this position - street, building number, postal code, city and country all together, or none of them to use the project's own workplace."
    )]
        string? PostalCode,
    [property: Description(
        "City. Workplace address of this position - street, building number, postal code, city and country all together, or none of them to use the project's own workplace."
    )]
        string? City,
    [property: Description(
        "Country, ISO 3166-1 alpha-2. Workplace address of this position - street, building number, postal code, city and country all together, or none of them to use the project's own workplace."
    )]
        string? CountryCode,
    [property: Description("Hours a week, greater than zero and at most 168.")]
        decimal? WeeklyHours,
    [property: Description("When a working day starts, as a time of day, e.g. \"07:00\".")]
        TimeOnly? WorkStartsAt,
    [property: Description("The schedule in words, e.g. \"Mon-Fri, two shifts\".")]
        string? WorkSchedule,
    [property: Description("Day of the month wages are paid by, 1 to 31.")] int? PayoutDay,
    [property: Description("Probation, in words, e.g. \"1 month\".")] string? ProbationPeriod,
    [property: Description("Notice period, in words, e.g. \"2 weeks\".")] string? NoticePeriod,
    [property: Description(
        "What comes on top of the rate, named: accommodation, transport, per diem."
    )]
        IReadOnlyList<string>? Allowances,
    [property: Description(
        "How many people the role is for, at least one. A target, not a forecast."
    )]
        int? PlannedHeadcount,
    [property: Description(
        "Suggested engagement type for people planned onto the role. Each assignment still states its own, since that is what keys their compliance."
    )]
        EngagementType? DefaultEngagementType
)
{
    public OpenPosition ToOpenCommand(
        Guid projectId,
        OrganizationId organizationId,
        Guid modifiedBy
    ) =>
        new(
            projectId,
            organizationId.Value,
            Name,
            ContractName,
            WorkDescription,
            Duties,
            RequiredQualifications,
            ContractType,
            RateAmount,
            RateCurrency,
            RateUnit,
            RateBasis,
            Street,
            BuildingNumber,
            UnitNumber,
            PostalCode,
            City,
            CountryCode,
            WeeklyHours,
            WorkStartsAt,
            WorkSchedule,
            PayoutDay,
            ProbationPeriod,
            NoticePeriod,
            Allowances,
            PlannedHeadcount,
            DefaultEngagementType,
            modifiedBy
        );
}
