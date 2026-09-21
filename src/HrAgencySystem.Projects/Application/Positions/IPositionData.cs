using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Application.Positions;

/// <summary>
/// What opening a position and editing one have in common, so one factory validates both - the
/// same arrangement <c>IProjectData</c> has.
/// </summary>
public interface IPositionData
{
    string Name { get; }
    string? ContractName { get; }
    string? WorkDescription { get; }
    IReadOnlyList<string>? Duties { get; }
    IReadOnlyList<string>? RequiredQualifications { get; }
    WorkerContractType ContractType { get; }
    decimal? RateAmount { get; }
    string? RateCurrency { get; }
    RateUnit RateUnit { get; }
    RateBasis RateBasis { get; }
    string? Street { get; }
    string? BuildingNumber { get; }
    string? UnitNumber { get; }
    string? PostalCode { get; }
    string? City { get; }
    string? CountryCode { get; }
    decimal? WeeklyHours { get; }
    TimeOnly? WorkStartsAt { get; }
    string? WorkSchedule { get; }
    int? PayoutDay { get; }
    string? ProbationPeriod { get; }
    string? NoticePeriod { get; }
    IReadOnlyList<string>? Allowances { get; }
    int? PlannedHeadcount { get; }
    EngagementType? DefaultEngagementType { get; }
}
