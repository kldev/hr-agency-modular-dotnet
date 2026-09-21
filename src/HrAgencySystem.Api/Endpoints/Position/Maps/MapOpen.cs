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
    string Name,
    string? ContractName,
    string? WorkDescription,
    IReadOnlyList<string>? Duties,
    IReadOnlyList<string>? RequiredQualifications,
    WorkerContractType ContractType,
    decimal? RateAmount,
    string? RateCurrency,
    RateUnit RateUnit,
    RateBasis RateBasis,
    string? Street,
    string? BuildingNumber,
    string? UnitNumber,
    string? PostalCode,
    string? City,
    string? CountryCode,
    decimal? WeeklyHours,
    TimeOnly? WorkStartsAt,
    string? WorkSchedule,
    int? PayoutDay,
    string? ProbationPeriod,
    string? NoticePeriod,
    IReadOnlyList<string>? Allowances,
    int? PlannedHeadcount,
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
